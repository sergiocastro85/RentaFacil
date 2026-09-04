using FluentAssertions;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.UnitTests.ValueObjects;

public class EmailTests
{
    [Fact]
    public void Create_ConFormatoValido_RetornaExito()
    {
        var resultado = Email.Create("cliente@rentafacil.com");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Valor.Should().Be("cliente@rentafacil.com");
    }

    [Theory]
    [InlineData("")]
    [InlineData("sin-arroba.com")]
    [InlineData("sin-dominio@")]
    [InlineData("@sin-usuario.com")]
    public void Create_ConFormatoInvalido_RetornaFallo(string valor)
    {
        var resultado = Email.Create(valor);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ClienteErrors.EmailInvalido);
    }

    [Fact]
    public void Create_ConMayusculas_NormalizaAMinusculas()
    {
        var resultado = Email.Create("Cliente@RentaFacil.COM");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Valor.Should().Be("cliente@rentafacil.com");
    }
}
