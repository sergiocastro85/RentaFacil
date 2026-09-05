using FluentAssertions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.Bloqueos.Commands.CrearBloqueo;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.UnitTests.Bloqueos.Commands.CrearBloqueo;

public class CrearBloqueoCommandHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IVehiculoRepository> _vehiculoRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly CrearBloqueoCommandHandler _handler;

    public CrearBloqueoCommandHandlerTests()
    {
        _dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(FechaActual);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task<Result<BloqueoResponse>>>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task<Result<BloqueoResponse>>>, CancellationToken>(
                (operation, cancellationToken) => operation(cancellationToken));

        _handler = new CrearBloqueoCommandHandler(
            _vehiculoRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object);
    }

    private static Vehiculo CrearVehiculo() => Vehiculo.Crear(
        Guid.NewGuid(),
        Placa.Create("ABC123").Value,
        TipoVehiculo.SUV,
        "Toyota",
        "Fortuner",
        2024,
        Dinero.Create(100_000m, "COP").Value,
        FechaActual).Value;

    [Fact]
    public async Task Handle_ConVehiculoDisponible_RetornaExito()
    {
        var vehiculo = CrearVehiculo();
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(vehiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculo);

        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var command = new CrearBloqueoCommand(vehiculo.Id, fechaInicio, fechaInicio.AddDays(5), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Placa.Should().Be("ABC123");
        result.Value.TarifaDiaria.Should().Be(100_000m);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConVehiculoInexistente_RetornaNotFound()
    {
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehiculo?)null);

        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var command = new CrearBloqueoCommand(Guid.NewGuid(), fechaInicio, fechaInicio.AddDays(5), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorType.Should().Be(ErrorType.NotFound);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConPeriodoSolapado_RetornaConflict()
    {
        var vehiculo = CrearVehiculo();
        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        vehiculo.AgregarBloqueo(
            Periodo.Create(fechaInicio, fechaInicio.AddDays(10), fechaInicio).Value,
            Guid.NewGuid(),
            FechaActual);

        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(vehiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculo);

        var command = new CrearBloqueoCommand(vehiculo.Id, fechaInicio.AddDays(5), fechaInicio.AddDays(15), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorType.Should().Be(ErrorType.Conflict);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConMismaReferenciaYMismoPeriodo_RetornaBloqueoExistente()
    {
        var vehiculo = CrearVehiculo();
        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var fechaFin = fechaInicio.AddDays(5);
        var referenciaExternaId = Guid.NewGuid();
        vehiculo.AgregarBloqueo(Periodo.Create(fechaInicio, fechaFin, fechaInicio).Value, referenciaExternaId, FechaActual);

        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(vehiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculo);

        var command = new CrearBloqueoCommand(vehiculo.Id, fechaInicio, fechaFin, referenciaExternaId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        vehiculo.Bloqueos.Should().HaveCount(1);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConPeriodoInvalido_RetornaFalloDeDominio()
    {
        var vehiculo = CrearVehiculo();
        var fechaInicio = DateOnly.FromDateTime(FechaActual).AddDays(-1);

        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(vehiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculo);

        var command = new CrearBloqueoCommand(vehiculo.Id, fechaInicio, fechaInicio.AddDays(5), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.PeriodoInvalido");
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
