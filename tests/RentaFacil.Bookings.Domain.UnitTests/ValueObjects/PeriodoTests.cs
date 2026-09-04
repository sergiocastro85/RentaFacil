using FluentAssertions;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.UnitTests.ValueObjects;

public class PeriodoTests
{
    private static readonly DateOnly Hoy = new(2026, 9, 1);

    [Fact]
    public void Create_ConFechaFinAnteriorAInicio_RetornaFallo()
    {
        var fechaInicio = Hoy.AddDays(5);
        var fechaFin = Hoy.AddDays(2);

        var resultado = Periodo.Create(fechaInicio, fechaFin, Hoy);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ReservaErrors.PeriodoInvalido);
    }

    [Fact]
    public void Create_ConFechaInicioEnElPasado_RetornaFallo()
    {
        var fechaInicio = Hoy.AddDays(-1);
        var fechaFin = Hoy.AddDays(5);

        var resultado = Periodo.Create(fechaInicio, fechaFin, Hoy);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ReservaErrors.PeriodoInvalido);
    }

    [Fact]
    public void Create_ConFechasValidas_RetornaExito()
    {
        var resultado = Periodo.Create(Hoy, Hoy.AddDays(5), Hoy);

        resultado.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Dias_ConPeriodoDeCincoDias_RetornaCinco()
    {
        var periodo = Periodo.Create(Hoy, Hoy.AddDays(5), Hoy).Value;

        periodo.Dias.Should().Be(5);
    }

    [Fact]
    public void SeSolapaCon_ConRangosQueSeCruzan_RetornaTrue()
    {
        var periodoA = Periodo.Create(Hoy, Hoy.AddDays(10), Hoy).Value;
        var periodoB = Periodo.Create(Hoy.AddDays(5), Hoy.AddDays(15), Hoy).Value;

        periodoA.SeSolapaCon(periodoB).Should().BeTrue();
    }

    [Fact]
    public void SeSolapaCon_ConRangoContenido_RetornaTrue()
    {
        var periodoA = Periodo.Create(Hoy, Hoy.AddDays(20), Hoy).Value;
        var periodoB = Periodo.Create(Hoy.AddDays(5), Hoy.AddDays(10), Hoy).Value;

        periodoA.SeSolapaCon(periodoB).Should().BeTrue();
    }

    [Fact]
    public void SeSolapaCon_ConRangosDisjuntos_RetornaFalse()
    {
        var periodoA = Periodo.Create(Hoy, Hoy.AddDays(5), Hoy).Value;
        var periodoB = Periodo.Create(Hoy.AddDays(10), Hoy.AddDays(15), Hoy).Value;

        periodoA.SeSolapaCon(periodoB).Should().BeFalse();
    }

    [Fact]
    public void SeSolapaCon_CuandoFinDeUnoEsInicioDeOtro_RetornaFalse()
    {
        var periodoA = Periodo.Create(Hoy, Hoy.AddDays(10), Hoy).Value;
        var periodoB = Periodo.Create(Hoy.AddDays(10), Hoy.AddDays(15), Hoy).Value;

        periodoA.SeSolapaCon(periodoB).Should().BeFalse();
    }
}
