using FluentAssertions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Vehicles.Application.Vehiculos.Commands.RegistrarVehiculo;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.UnitTests.Vehiculos.Commands.RegistrarVehiculo;

public class RegistrarVehiculoCommandHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IVehiculoRepository> _vehiculoRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly RegistrarVehiculoCommandHandler _handler;

    public RegistrarVehiculoCommandHandlerTests()
    {
        _dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(FechaActual);
        _handler = new RegistrarVehiculoCommandHandler(
            _vehiculoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    [Fact]
    public async Task Handle_ConDatosValidos_RetornaExito()
    {
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehiculo?)null);

        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 100_000m, "COP");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Placa.Should().Be("ABC123");
        result.Value.TarifaDiaria.Should().Be(100_000m);
        _vehiculoRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Vehiculo>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConPlacaDuplicada_RetornaConflict()
    {
        var vehiculoExistente = Vehiculo.Crear(
            Guid.NewGuid(),
            Placa.Create("ABC123").Value,
            TipoVehiculo.SUV,
            "Toyota",
            "Fortuner",
            2024,
            Dinero.Create(100_000m, "COP").Value,
            FechaActual).Value;

        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculoExistente);

        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 100_000m, "COP");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.PlacaDuplicada");
        _vehiculoRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Vehiculo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConAnioFueraDeRango_RetornaFalloDeDominio()
    {
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehiculo?)null);

        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 1980, 100_000m, "COP");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.AnioFueraDeRango");
        _vehiculoRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Vehiculo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConPlacaInvalida_RetornaFalloDeDominioSinConsultarRepositorio()
    {
        var command = new RegistrarVehiculoCommand("AB", TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 100_000m, "COP");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.PlacaInvalida");
        _vehiculoRepositoryMock.Verify(
            repository => repository.ObtenerPorPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConMonedaInvalida_RetornaFalloDeDominio()
    {
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorPlacaAsync(It.IsAny<Placa>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehiculo?)null);

        var command = new RegistrarVehiculoCommand("ABC123", TipoVehiculo.SUV, "Toyota", "Fortuner", 2024, 100_000m, "XX");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.TarifaInvalida");
        _vehiculoRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Vehiculo>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
