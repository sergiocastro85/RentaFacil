using FluentAssertions;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.UnitTests.Entities;

public class VehiculoTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);
    private static readonly DateOnly Hoy = DateOnly.FromDateTime(FechaActual);

    private static Placa PlacaValida() => Placa.Create("ABC123").Value;

    private static Dinero TarifaValida() => Dinero.Create(100_000m, "COP").Value;

    private static Vehiculo VehiculoValido() => Vehiculo.Crear(
        Guid.NewGuid(),
        PlacaValida(),
        TipoVehiculo.SUV,
        "Toyota",
        "Fortuner",
        2024,
        TarifaValida(),
        FechaActual).Value;

    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var resultado = Vehiculo.Crear(
            Guid.NewGuid(),
            PlacaValida(),
            TipoVehiculo.SUV,
            "Toyota",
            "Fortuner",
            2024,
            TarifaValida(),
            FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Anio.Should().Be(2024);
        resultado.Value.Bloqueos.Should().BeEmpty();
    }

    [Fact]
    public void Crear_ConAnioAnteriorA1990_RetornaFallo()
    {
        var resultado = Vehiculo.Crear(
            Guid.NewGuid(),
            PlacaValida(),
            TipoVehiculo.Sedan,
            "Renault",
            "Twingo",
            1989,
            TarifaValida(),
            FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.AnioFueraDeRango);
    }

    [Fact]
    public void Crear_ConAnioMayorAlActualMasUno_RetornaFallo()
    {
        var resultado = Vehiculo.Crear(
            Guid.NewGuid(),
            PlacaValida(),
            TipoVehiculo.Sedan,
            "Renault",
            "Twingo",
            FechaActual.Year + 2,
            TarifaValida(),
            FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.AnioFueraDeRango);
    }

    [Fact]
    public void Crear_ConTarifaCeroONegativa_RetornaFallo()
    {
        var tarifaCero = Dinero.Create(0m, "COP").Value;

        var resultado = Vehiculo.Crear(
            Guid.NewGuid(),
            PlacaValida(),
            TipoVehiculo.Van,
            "Chevrolet",
            "N300",
            2020,
            tarifaCero,
            FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.TarifaInvalida);
    }

    [Fact]
    public void AgregarBloqueo_ComoPrimerBloqueo_RetornaExito()
    {
        var vehiculo = VehiculoValido();
        var periodo = Periodo.Create(Hoy, Hoy.AddDays(5), Hoy).Value;

        var resultado = vehiculo.AgregarBloqueo(periodo, Guid.NewGuid(), FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        vehiculo.Bloqueos.Should().HaveCount(1);
    }

    [Fact]
    public void AgregarBloqueo_ConPeriodoSolapado_RetornaFallo()
    {
        var vehiculo = VehiculoValido();
        var primerPeriodo = Periodo.Create(Hoy, Hoy.AddDays(10), Hoy).Value;
        vehiculo.AgregarBloqueo(primerPeriodo, Guid.NewGuid(), FechaActual);

        var periodoSolapado = Periodo.Create(Hoy.AddDays(5), Hoy.AddDays(15), Hoy).Value;
        var resultado = vehiculo.AgregarBloqueo(periodoSolapado, Guid.NewGuid(), FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Code.Should().Be("Vehiculo.NoDisponible");
        vehiculo.Bloqueos.Should().HaveCount(1);
    }

    [Fact]
    public void AgregarBloqueo_ConPeriodoAdyacente_RetornaExito()
    {
        var vehiculo = VehiculoValido();
        var primerPeriodo = Periodo.Create(Hoy, Hoy.AddDays(10), Hoy).Value;
        vehiculo.AgregarBloqueo(primerPeriodo, Guid.NewGuid(), FechaActual);

        var periodoAdyacente = Periodo.Create(Hoy.AddDays(10), Hoy.AddDays(15), Hoy).Value;
        var resultado = vehiculo.AgregarBloqueo(periodoAdyacente, Guid.NewGuid(), FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        vehiculo.Bloqueos.Should().HaveCount(2);
    }
}
