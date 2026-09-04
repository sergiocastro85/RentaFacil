using FluentAssertions;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Domain.UnitTests.Entities;

public class ClienteTests
{
    private static readonly DateTime FechaActual = new(2026, 9, 1, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Crear_ConDatosValidos_RetornaExito()
    {
        var resultado = Cliente.Crear(
            Guid.NewGuid(),
            "CC",
            "1234567890",
            "Juan Pérez",
            "juan.perez@rentafacil.com",
            "3001234567",
            FechaActual);

        resultado.IsSuccess.Should().BeTrue();
        resultado.Value.Documento.Numero.Should().Be("1234567890");
        resultado.Value.Email.Valor.Should().Be("juan.perez@rentafacil.com");
    }

    [Fact]
    public void Crear_ConDocumentoInvalido_PropagaFalloDelVO()
    {
        var resultado = Cliente.Crear(
            Guid.NewGuid(),
            "CC",
            "ABC",
            "Juan Pérez",
            "juan.perez@rentafacil.com",
            "3001234567",
            FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ClienteErrors.DocumentoInvalido);
    }

    [Fact]
    public void Crear_ConEmailInvalido_PropagaFalloDelVO()
    {
        var resultado = Cliente.Crear(
            Guid.NewGuid(),
            "CC",
            "1234567890",
            "Juan Pérez",
            "correo-invalido",
            "3001234567",
            FechaActual);

        resultado.IsFailure.Should().BeTrue();
        resultado.Error.Should().Be(ClienteErrors.EmailInvalido);
    }
}
