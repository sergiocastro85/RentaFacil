using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.Abstractions;
using RentaFacil.Bookings.Application.Reservas.Commands.CrearReserva;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Application.UnitTests.Reservas.Commands.CrearReserva;

public class CrearReservaCommandHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(FechaActual);

    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly Mock<IReservaRepository> _reservaRepositoryMock = new();
    private readonly Mock<IVehicleCatalogService> _vehicleCatalogServiceMock = new();
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<IDateTimeProvider> _dateTimeProviderMock = new();
    private readonly CrearReservaCommandHandler _handler;

    public CrearReservaCommandHandlerTests()
    {
        _dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(FechaActual);
        _handler = new CrearReservaCommandHandler(
            _clienteRepositoryMock.Object,
            _reservaRepositoryMock.Object,
            _vehicleCatalogServiceMock.Object,
            _unitOfWorkMock.Object,
            _dateTimeProviderMock.Object,
            NullLogger<CrearReservaCommandHandler>.Instance);
    }

    private static Cliente CrearCliente() => Cliente.Crear(
        Guid.NewGuid(),
        "CC",
        "1234567890",
        "Juan Pérez",
        "juan.perez@rentafacil.com",
        "3001234567",
        FechaActual).Value;

    private static CrearReservaCommand CrearComando(Guid clienteId, Guid vehiculoId) =>
        new(clienteId, vehiculoId, Hoy, Hoy.AddDays(5));

    [Fact]
    public async Task Handle_ConCaminoFeliz_RetornaExitoYNoInvocaLiberarCupo()
    {
        var cliente = CrearCliente();
        var vehiculoId = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var cupo = new CupoReservadoDto(Guid.NewGuid(), "ABC123", TipoVehiculo.SUV, 100_000m, "COP");
        _vehicleCatalogServiceMock
            .Setup(service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cupo);

        var command = CrearComando(cliente.Id, vehiculoId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.PlacaVehiculo.Should().Be("ABC123");
        result.Value.ValorTotal.Should().Be(500_000m);

        _vehicleCatalogServiceMock.Verify(
            service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _reservaRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()),
            Times.Once);
        _unitOfWorkMock.Verify(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        _vehicleCatalogServiceMock.Verify(
            service => service.LiberarCupoAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConClienteInexistente_RetornaNotFoundYNoInvocaReservarCupo()
    {
        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var command = CrearComando(Guid.NewGuid(), Guid.NewGuid());

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorType.Should().Be(ErrorType.NotFound);
        _vehicleCatalogServiceMock.Verify(
            service => service.ReservarCupoAsync(
                It.IsAny<Guid>(), It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConVehiculoNoDisponible_PropagaConflictYNoPersisteNada()
    {
        var cliente = CrearCliente();
        var vehiculoId = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _vehicleCatalogServiceMock
            .Setup(service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CupoReservadoDto>(ReservaErrors.VehiculoNoDisponible(vehiculoId)));

        var command = CrearComando(cliente.Id, vehiculoId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorType.Should().Be(ErrorType.Conflict);
        _reservaRepositoryMock.Verify(
            repository => repository.AgregarAsync(It.IsAny<Reserva>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_ConVehiculoInexistente_PropagaNotFound()
    {
        var cliente = CrearCliente();
        var vehiculoId = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _vehicleCatalogServiceMock
            .Setup(service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CupoReservadoDto>(ReservaErrors.VehiculoNoEncontrado(vehiculoId)));

        var command = CrearComando(cliente.Id, vehiculoId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.ErrorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public async Task Handle_ConFalloDeComunicacion_PropagaFailure()
    {
        var cliente = CrearCliente();
        var vehiculoId = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _vehicleCatalogServiceMock
            .Setup(service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure<CupoReservadoDto>(ReservaErrors.FalloComunicacionVehicleService));

        var command = CrearComando(cliente.Id, vehiculoId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservaErrors.FalloComunicacionVehicleService);
    }

    [Fact]
    public async Task Handle_ConFalloAlPersistir_InvocaLiberarCupoUnaVezYRetornaFailure()
    {
        var cliente = CrearCliente();
        var vehiculoId = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var cupo = new CupoReservadoDto(Guid.NewGuid(), "ABC123", TipoVehiculo.SUV, 100_000m, "COP");
        _vehicleCatalogServiceMock
            .Setup(service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cupo);

        _vehicleCatalogServiceMock
            .Setup(service => service.LiberarCupoAsync(vehiculoId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fallo simulado de base de datos."));

        var command = CrearComando(cliente.Id, vehiculoId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservaErrors.FalloAlPersistir);
        _vehicleCatalogServiceMock.Verify(
            service => service.LiberarCupoAsync(vehiculoId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ConFalloAlPersistirYFalloEnCompensacion_RetornaFailureSinLanzarExcepcion()
    {
        var cliente = CrearCliente();
        var vehiculoId = Guid.NewGuid();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        var cupo = new CupoReservadoDto(Guid.NewGuid(), "ABC123", TipoVehiculo.SUV, 100_000m, "COP");
        _vehicleCatalogServiceMock
            .Setup(service => service.ReservarCupoAsync(
                vehiculoId, It.IsAny<Periodo>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cupo);

        _vehicleCatalogServiceMock
            .Setup(service => service.LiberarCupoAsync(vehiculoId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Failure(ReservaErrors.FalloComunicacionVehicleService));

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Fallo simulado de base de datos."));

        var command = CrearComando(cliente.Id, vehiculoId);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(ReservaErrors.FalloAlPersistir);
        _vehicleCatalogServiceMock.Verify(
            service => service.LiberarCupoAsync(vehiculoId, It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
