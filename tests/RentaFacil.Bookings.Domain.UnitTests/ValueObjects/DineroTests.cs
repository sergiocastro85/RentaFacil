using FluentAssertions;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.UnitTests.ValueObjects;

public class DineroTests
{
    [Fact]
    public void Create_ConMontoNegativo_RetornaFallo()
    {
        var resultado = Dinero.Create(-1m, "COP");

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ReservaErrors.TarifaInvalida);
    }

    [Fact]
    public void Create_ConMontoCero_RetornaExito()
    {
        var resultado = Dinero.Create(0m, "COP");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Monto.Should().Be(0m);
    }

    [Fact]
    public void Sumar_ConMonedasDistintas_RetornaFallo()
    {
        var dineroA = Dinero.Create(100m, "COP").Value;
        var dineroB = Dinero.Create(50m, "USD").Value;

        var resultado = dineroA.Sumar(dineroB);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ReservaErrors.TarifaInvalida);
    }

    [Fact]
    public void MultiplicarPor_ConFactorPositivo_MultiplicaElMonto()
    {
        var dinero = Dinero.Create(100_000m, "COP").Value;

        var resultado = dinero.MultiplicarPor(5);

        resultado.Monto.Should().Be(500_000m);
        resultado.Moneda.Should().Be("COP");
    }
}
