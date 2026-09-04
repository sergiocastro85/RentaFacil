using FluentAssertions;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.UnitTests.ValueObjects;

public class PlacaTests
{
    [Fact]
    public void Create_ConFormatoValido_RetornaExito()
    {
        var resultado = Placa.Create("ABC123");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Valor.Should().Be("ABC123");
    }

    [Theory]
    [InlineData("")]
    [InlineData("AB")]
    [InlineData("ABCDEFGHIJK")]
    [InlineData("ABC-123")]
    public void Create_ConFormatoInvalido_RetornaFallo(string valor)
    {
        var resultado = Placa.Create(valor);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(VehiculoErrors.PlacaInvalida);
    }

    [Fact]
    public void Create_ConMinusculasYEspacios_NormalizaAMayusculasSinEspacios()
    {
        var resultado = Placa.Create(" abc 123 ");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Valor.Should().Be("ABC123");
    }
}
