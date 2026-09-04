using FluentAssertions;
using Moq;
using RentaFacil.Bookings.Application.Reservas.Queries.ObtenerHistorialPorCliente;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Application.UnitTests.Reservas.Queries.ObtenerHistorialPorCliente;

public class ObtenerHistorialPorClienteQueryHandlerTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(FechaActual);

    private readonly Mock<IClienteRepository> _clienteRepositoryMock = new();
    private readonly Mock<IReservaRepository> _reservaRepositoryMock = new();
    private readonly ObtenerHistorialPorClienteQueryHandler _handler;

    public ObtenerHistorialPorClienteQueryHandlerTests()
    {
        _handler = new ObtenerHistorialPorClienteQueryHandler(
            _clienteRepositoryMock.Object,
            _reservaRepositoryMock.Object);
    }

    private static Cliente CrearCliente() => Cliente.Crear(
        Guid.NewGuid(),
        "CC",
        "1234567890",
        "Juan Pérez",
        "juan.perez@rentafacil.com",
        "3001234567",
        FechaActual).Value;

    private static Reserva CrearReserva(Guid clienteId) => Reserva.Crear(
        Guid.NewGuid(),
        clienteId,
        Guid.NewGuid(),
        TipoVehiculo.SUV,
        "ABC123",
        Hoy,
        Hoy.AddDays(5),
        100_000m,
        "COP",
        FechaActual).Value;

    [Fact]
    public async Task Handle_ConClienteExistente_RetornaHistorial()
    {
        var cliente = CrearCliente();
        var reserva = CrearReserva(cliente.Id);

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _reservaRepositoryMock
            .Setup(repository => repository.ObtenerHistorialPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reserva> { reserva });

        var query = new ObtenerHistorialPorClienteQuery(cliente.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().HaveCount(1);
        result.Value[0].PlacaVehiculo.Should().Be("ABC123");
    }

    [Fact]
    public async Task Handle_ConClienteInexistente_RetornaNotFound()
    {
        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cliente?)null);

        var query = new ObtenerHistorialPorClienteQuery(Guid.NewGuid());

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("Reserva.ClienteNoEncontrado");
    }

    [Fact]
    public async Task Handle_ConClienteSinReservas_RetornaListaVaciaConExito()
    {
        var cliente = CrearCliente();

        _clienteRepositoryMock
            .Setup(repository => repository.ObtenerPorIdAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cliente);

        _reservaRepositoryMock
            .Setup(repository => repository.ObtenerHistorialPorClienteAsync(cliente.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Reserva>());

        var query = new ObtenerHistorialPorClienteQuery(cliente.Id);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEmpty();
    }
}
