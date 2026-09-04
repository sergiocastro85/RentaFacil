using System.Text.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Bookings.Application.Reportes.Commands.GenerarReporteDiario;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Infrastructure.Persistence;
using RentaFacil.Bookings.Infrastructure.Persistence.Repositories;

namespace RentaFacil.Reporting.UnitTests;

public class GenerarReporteDiarioCommandHandlerTests
{
    private static BookingsDbContext CrearDbContext()
    {
        var options = new DbContextOptionsBuilder<BookingsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new BookingsDbContext(options);
    }

    private static Reserva CrearReserva(DateTime fechaCreacion, TipoVehiculo tipo, Guid clienteId, decimal tarifaDiaria)
    {
        var fechaInicio = DateOnly.FromDateTime(fechaCreacion).AddDays(5);

        return Reserva.Crear(
            Guid.NewGuid(),
            clienteId,
            Guid.NewGuid(),
            tipo,
            "ABC123",
            fechaInicio,
            fechaInicio.AddDays(1),
            tarifaDiaria,
            "COP",
            fechaCreacion).Value;
    }

    private static GenerarReporteDiarioCommandHandler CrearHandler(BookingsDbContext dbContext, DateTime? fechaActual = null)
    {
        var repository = new ReporteRepository(dbContext);
        var unitOfWork = (IUnitOfWork)dbContext;

        var dateTimeProviderMock = new Mock<IDateTimeProvider>();
        dateTimeProviderMock.Setup(provider => provider.UtcNow).Returns(fechaActual ?? new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc));

        return new GenerarReporteDiarioCommandHandler(
            repository,
            unitOfWork,
            dateTimeProviderMock.Object,
            NullLogger<GenerarReporteDiarioCommandHandler>.Instance);
    }

    [Fact]
    public async Task Handle_ConVariasReservasDelDia_CalculaLosTotalesEsperados()
    {
        using var dbContext = CrearDbContext();

        var fecha = new DateOnly(2026, 9, 1);
        var clienteA = Guid.NewGuid();
        var clienteB = Guid.NewGuid();

        dbContext.Reservas.AddRange(
            CrearReserva(new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, clienteA, 100_000m),
            CrearReserva(new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, clienteA, 100_000m),
            CrearReserva(new DateTime(2026, 9, 1, 14, 0, 0, DateTimeKind.Utc), TipoVehiculo.Sedan, clienteB, 50_000m));
        await dbContext.SaveChangesAsync();

        var handler = CrearHandler(dbContext);
        var result = await handler.Handle(new GenerarReporteDiarioCommand(fecha), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalReservas.Should().Be(3);
        result.Value.ValorTotalReservado.Should().Be(250_000m);
        result.Value.TipoVehiculoMasReservado.Should().Be("SUV");
        result.Value.ClientesUnicos.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ConReservasFueraDeLaVentana_LasExcluyeEnAmbosBordes()
    {
        using var dbContext = CrearDbContext();

        var fecha = new DateOnly(2026, 9, 1);
        var clienteId = Guid.NewGuid();

        dbContext.Reservas.AddRange(
            // Justo antes del borde inferior: pertenece al 31 de agosto, no al 1 de septiembre.
            CrearReserva(new DateTime(2026, 8, 31, 23, 59, 59, DateTimeKind.Utc), TipoVehiculo.SUV, clienteId, 100_000m),
            // Dentro de la ventana.
            CrearReserva(new DateTime(2026, 9, 1, 12, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, clienteId, 100_000m),
            // Justo en el borde superior (exclusivo): pertenece al 2 de septiembre.
            CrearReserva(new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, clienteId, 100_000m));
        await dbContext.SaveChangesAsync();

        var handler = CrearHandler(dbContext);
        var result = await handler.Handle(new GenerarReporteDiarioCommand(fecha), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalReservas.Should().Be(1);
        result.Value.ValorTotalReservado.Should().Be(100_000m);
    }

    [Fact]
    public async Task Handle_SinReservasEnElDia_RetornaReporteEnCerosConExito()
    {
        using var dbContext = CrearDbContext();

        var handler = CrearHandler(dbContext);
        var result = await handler.Handle(new GenerarReporteDiarioCommand(new DateOnly(2026, 9, 1)), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.TotalReservas.Should().Be(0);
        result.Value.ValorTotalReservado.Should().Be(0m);
        result.Value.ClientesUnicos.Should().Be(0);
        result.Value.TipoVehiculoMasReservado.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EjecutadoDosVecesParaLaMismaFecha_DejaUnSoloRegistroActualizado()
    {
        using var dbContext = CrearDbContext();

        var fecha = new DateOnly(2026, 9, 1);
        var clienteId = Guid.NewGuid();

        dbContext.Reservas.Add(
            CrearReserva(new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, clienteId, 100_000m));
        await dbContext.SaveChangesAsync();

        var handler = CrearHandler(dbContext);
        var primerResultado = await handler.Handle(new GenerarReporteDiarioCommand(fecha), CancellationToken.None);
        primerResultado.Value.TotalReservas.Should().Be(1);

        dbContext.Reservas.Add(
            CrearReserva(new DateTime(2026, 9, 1, 15, 0, 0, DateTimeKind.Utc), TipoVehiculo.Sedan, Guid.NewGuid(), 60_000m));
        await dbContext.SaveChangesAsync();

        var segundoResultado = await handler.Handle(new GenerarReporteDiarioCommand(fecha), CancellationToken.None);

        segundoResultado.IsSuccess.Should().BeTrue();
        segundoResultado.Value.TotalReservas.Should().Be(2);

        var reportesEnBaseDeDatos = await dbContext.ReportesReservasDiarias
            .Where(reporte => reporte.Fecha == fecha)
            .ToListAsync();

        reportesEnBaseDeDatos.Should().HaveCount(1);
        reportesEnBaseDeDatos[0].TotalReservas.Should().Be(2);
    }

    [Fact]
    public async Task Handle_ConVariosTipos_DetalleJsonContieneElDesgloseEsperado()
    {
        using var dbContext = CrearDbContext();

        var fecha = new DateOnly(2026, 9, 1);

        dbContext.Reservas.AddRange(
            CrearReserva(new DateTime(2026, 9, 1, 8, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, Guid.NewGuid(), 100_000m),
            CrearReserva(new DateTime(2026, 9, 1, 9, 0, 0, DateTimeKind.Utc), TipoVehiculo.SUV, Guid.NewGuid(), 100_000m),
            CrearReserva(new DateTime(2026, 9, 1, 10, 0, 0, DateTimeKind.Utc), TipoVehiculo.Sedan, Guid.NewGuid(), 50_000m));
        await dbContext.SaveChangesAsync();

        var handler = CrearHandler(dbContext);
        var result = await handler.Handle(new GenerarReporteDiarioCommand(fecha), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();

        var desglose = JsonSerializer.Deserialize<Dictionary<string, int>>(result.Value.DetalleJson);

        desglose.Should().NotBeNull();
        desglose!["SUV"].Should().Be(2);
        desglose["Sedan"].Should().Be(1);
    }
}
