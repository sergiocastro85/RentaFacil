using FluentAssertions;
using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Domain.UnitTests.Entities;

public class ReporteReservasDiariasTests
{
    private static readonly DateTime FechaProcesamiento = new(2026, 9, 1, 23, 55, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_ConDatosValidos_AsignaCampos()
    {
        var fecha = new DateOnly(2026, 9, 1);

        var reporte = ReporteReservasDiarias.Crear(
            Guid.NewGuid(),
            fecha,
            totalReservas: 10,
            valorTotalReservado: 1_000_000m,
            tipoVehiculoMasReservado: "SUV",
            clientesUnicos: 7,
            detalleJson: "{}",
            fechaProcesamiento: FechaProcesamiento);

        reporte.Fecha.Should().Be(fecha);
        reporte.TotalReservas.Should().Be(10);
        reporte.ValorTotalReservado.Should().Be(1_000_000m);
        reporte.TipoVehiculoMasReservado.Should().Be("SUV");
        reporte.ClientesUnicos.Should().Be(7);
    }

    [Fact]
    public void ActualizarAgregados_AlReprocesar_SobrescribeValores()
    {
        var fecha = new DateOnly(2026, 9, 1);
        var reporte = ReporteReservasDiarias.Crear(
            Guid.NewGuid(), fecha, 10, 1_000_000m, "SUV", 7, "{}", FechaProcesamiento);

        var nuevaFechaProcesamiento = FechaProcesamiento.AddMinutes(10);

        reporte.ActualizarAgregados(
            totalReservas: 12,
            valorTotalReservado: 1_500_000m,
            tipoVehiculoMasReservado: "Sedan",
            clientesUnicos: 9,
            detalleJson: "{\"Sedan\":12}",
            fechaProcesamiento: nuevaFechaProcesamiento);

        reporte.Fecha.Should().Be(fecha);
        reporte.TotalReservas.Should().Be(12);
        reporte.ValorTotalReservado.Should().Be(1_500_000m);
        reporte.TipoVehiculoMasReservado.Should().Be("Sedan");
        reporte.ClientesUnicos.Should().Be(9);
        reporte.DetalleJson.Should().Be("{\"Sedan\":12}");
        reporte.FechaProcesamiento.Should().Be(nuevaFechaProcesamiento);
    }
}
