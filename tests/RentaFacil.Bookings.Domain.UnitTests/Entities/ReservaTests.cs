using FluentAssertions;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Domain.UnitTests.Entities;

public class ReservaTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(FechaActual);

    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var resultado = Reserva.Crear(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TipoVehiculo.Sedan,
            "ABC123",
            Hoy,
            Hoy.AddDays(5),
            100_000m,
            "COP",
            FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.PlacaVehiculo.Should().Be("ABC123");
        resultado.Value.TipoVehiculo.Should().Be(TipoVehiculo.Sedan);
    }

    [Fact]
    public void Crear_ConPeriodoDeUnDia_CalculaValorTotalCorrectamente()
    {
        var resultado = Reserva.Crear(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoVehiculo.Sedan, "ABC123",
            Hoy, Hoy.AddDays(1), 100_000m, "COP", FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.ValorTotal.Monto.Should().Be(100_000m);
    }

    [Fact]
    public void Crear_ConPeriodoDeCincoDias_CalculaValorTotalCorrectamente()
    {
        var resultado = Reserva.Crear(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoVehiculo.Sedan, "ABC123",
            Hoy, Hoy.AddDays(5), 100_000m, "COP", FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.ValorTotal.Monto.Should().Be(500_000m);
    }

    [Fact]
    public void Crear_ConPeriodoQueCruzaDeMes_CalculaValorTotalCorrectamente()
    {
        var fechaInicio = new DateOnly(2026, 9, 28);
        var fechaFin = new DateOnly(2026, 10, 3);

        var resultado = Reserva.Crear(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoVehiculo.Sedan, "ABC123",
            fechaInicio, fechaFin, 100_000m, "COP", FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Periodo.Dias.Should().Be(5);
        resultado.Value.ValorTotal.Monto.Should().Be(500_000m);
    }

    [Fact]
    public void Crear_ConPeriodoInvalido_RetornaFallo()
    {
        var resultado = Reserva.Crear(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoVehiculo.Sedan, "ABC123",
            Hoy.AddDays(5), Hoy.AddDays(2), 100_000m, "COP", FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ReservaErrors.PeriodoInvalido);
    }

    [Fact]
    public void Crear_ConTarifaInvalida_RetornaFallo()
    {
        var resultado = Reserva.Crear(
            Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), TipoVehiculo.Sedan, "ABC123",
            Hoy, Hoy.AddDays(5), -1m, "COP", FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ReservaErrors.TarifaInvalida);
    }

    [Fact]
    public void Crear_ConDatosValidos_CongelaPlacaYTarifaComoSnapshot()
    {
        var vehiculoId = Guid.NewGuid();

        var resultado = Reserva.Crear(
            Guid.NewGuid(), Guid.NewGuid(), vehiculoId, TipoVehiculo.SUV, "XYZ789",
            Hoy, Hoy.AddDays(3), 150_000m, "COP", FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        var reserva = resultado.Value;

        reserva.VehiculoId.Should().Be(vehiculoId);
        reserva.TipoVehiculo.Should().Be(TipoVehiculo.SUV);
        reserva.PlacaVehiculo.Should().Be("XYZ789");
        reserva.TarifaDiariaAplicada.Monto.Should().Be(150_000m);
        reserva.TarifaDiariaAplicada.Moneda.Should().Be("COP");
    }
}
