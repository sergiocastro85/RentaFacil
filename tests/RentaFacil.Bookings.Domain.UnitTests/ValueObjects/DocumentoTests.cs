using FluentAssertions;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.UnitTests.ValueObjects;

public class DocumentoTests
{
    [Fact]
    public void Create_ConNumeroValido_RetornaExito()
    {
        var resultado = Documento.Create("CC", "1234567890");

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Numero.Should().Be("1234567890");
        resultado.Value.Tipo.Should().Be("CC");
    }

    [Fact]
    public void Create_ConNumeroConLetras_RetornaFallo()
    {
        var resultado = Documento.Create("CC", "123ABC789");

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ClienteErrors.DocumentoInvalido);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("1234567890123456")]
    public void Create_ConLongitudFueraDeRango_RetornaFallo(string numero)
    {
        var resultado = Documento.Create("CC", numero);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ClienteErrors.DocumentoInvalido);
    }

    [Fact]
    public void Create_ConTipoVacio_RetornaFallo()
    {
        var resultado = Documento.Create("", "1234567890");

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ClienteErrors.DocumentoInvalido);
    }
}
