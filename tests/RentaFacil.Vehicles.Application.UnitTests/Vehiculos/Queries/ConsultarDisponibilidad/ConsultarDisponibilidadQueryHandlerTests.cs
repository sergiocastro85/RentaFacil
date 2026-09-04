using FluentAssertions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.Vehiculos.Queries.ConsultarDisponibilidad;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.UnitTests.Vehiculos.Queries.ConsultarDisponibilidad;

public class ConsultarDisponibilidadQueryHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IVehiculoRepository> _vehiculoRepositoryMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly ConsultarDisponibilidadQueryHandler _handler;

    public ConsultarDisponibilidadQueryHandlerTests()
    {
        _dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(FechaActual);
        _handler = new ConsultarDisponibilidadQueryHandler(_vehiculoRepositoryMock.Object, _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ConVehiculosDisponibles_RetornaListaDelRepositorio()
    {
        var vehiculo = Vehiculo.Crear(
            Guid.NewGuid(),
            Placa.Create("ABC123").Value,
            TipoVehiculo.SUV,
            "Toyota",
            "Fortuner",
            2024,
            Dinero.Create(100_000m, "COP").Value,
            FechaActual).Value;

        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerDisponiblesAsync(
                TipoVehiculo.SUV, It.IsAny<Periodo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vehiculo> { vehiculo });

        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var query = new ConsultarDisponibilidadQuery(TipoVehiculo.SUV, fechaInicio, fechaInicio.AddDays(5));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].Placa.Should().Be("ABC123");
    }

    [Fact]
    public async Task Handle_ConPeriodoInvalido_RetornaValidation()
    {
        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var query = new ConsultarDisponibilidadQuery(TipoVehiculo.SUV, fechaInicio, fechaInicio.AddDays(-1));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorType.Should().Be(ErrorType.Validation);
        _vehiculoRepositoryMock.Verify(
            repository => repository.ObtenerDisponiblesAsync(
                It.IsAny<TipoVehiculo>(), It.IsAny<Periodo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_SinResultados_RetornaListaVacia()
    {
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerDisponiblesAsync(
                It.IsAny<TipoVehiculo>(), It.IsAny<Periodo>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Vehiculo>());

        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var query = new ConsultarDisponibilidadQuery(TipoVehiculo.SUV, fechaInicio, fechaInicio.AddDays(5));

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
