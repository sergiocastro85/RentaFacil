using FluentAssertions;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.UnitTests.ValueObjects;

public class DineroTests
{
    [Fact]
    public void Create_ConMontoNegativo_RetornaFallo()
    {
        var resultado = Dinero.Create(-1m, "COP");

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.TarifaInvalida);
    }

    [Fact]
    public void Create_ConMontoCero_RetornaExito()
    {
        var resultado = Dinero.Create(0m, "COP");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Monto.Should().Be(0m);
    }

    [Theory]
    [InlineData("")]
    [InlineData("CO")]
    [InlineData("COPX")]
    [InlineData("C0P")]
    public void Create_ConMonedaInvalida_RetornaFallo(string moneda)
    {
        var resultado = Dinero.Create(100m, moneda);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.TarifaInvalida);
    }

    [Fact]
    public void Sumar_ConMonedasDistintas_RetornaFallo()
    {
        var dineroA = Dinero.Create(100m, "COP").Value;
        var dineroB = Dinero.Create(50m, "USD").Value;

        var resultado = dineroA.Sumar(dineroB);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.TarifaInvalida);
    }

    [Fact]
    public void Sumar_ConMonedasIguales_SumaLosMontos()
    {
        var dineroA = Dinero.Create(100m, "COP").Value;
        var dineroB = Dinero.Create(50m, "COP").Value;

        var resultado = dineroA.Sumar(dineroB);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Monto.Should().Be(150m);
        resultado.Value.Moneda.Should().Be("COP");
    }
}
