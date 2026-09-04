using FluentAssertions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Vehicles.Application.Bloqueos.Commands.LiberarBloqueo;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.UnitTests.Bloqueos.Commands.LiberarBloqueo;

public class LiberarBloqueoCommandHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    private readonly Mock<IVehiculoRepository> _vehiculoRepositoryMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly LiberarBloqueoCommandHandler _handler;

    public LiberarBloqueoCommandHandlerTests()
    {
        _handler = new LiberarBloqueoCommandHandler(_vehiculoRepositoryMock.Object, _unitOfWorkMock.Object);
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
    public async Task Handle_ConBloqueoExistente_RetornaExito()
    {
        var vehiculo = CrearVehiculo();
        var fechaInicio = DateOnly.FromDateTime(FechaActual);
        var referenciaExternaId = Guid.NewGuid();
        vehiculo.AgregarBloqueo(Periodo.Create(fechaInicio, fechaInicio.AddDays(5), fechaInicio).Value, referenciaExternaId, FechaActual);

        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(vehiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculo);

        var command = new LiberarBloqueoCommand(vehiculo.Id, referenciaExternaId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        vehiculo.Bloqueos.Should().BeEmpty();
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ConVehiculoInexistente_RetornaNotFound()
    {
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Vehiculo?)null);

        var command = new LiberarBloqueoCommand(Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.NoEncontrado");
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ConBloqueoInexistente_RetornaNotFound()
    {
        var vehiculo = CrearVehiculo();
        _vehiculoRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(vehiculo.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(vehiculo);

        var command = new LiberarBloqueoCommand(vehiculo.Id, Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Vehiculo.BloqueoNoEncontrado");
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}
