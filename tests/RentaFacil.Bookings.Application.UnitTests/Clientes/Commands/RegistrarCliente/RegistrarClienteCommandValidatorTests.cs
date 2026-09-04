using FluentValidation.TestHelper;
using RentaFacil.Bookings.Application.Clientes.Commands.RegistrarCliente;

namespace RentaFacil.Bookings.Application.UnitTests.Clientes.Commands.RegistrarCliente;

public class RegistrarClienteCommandValidatorTests
{
    private readonly RegistrarClienteCommandValidator _validator = new();

    [Fact]
    public void Validate_ConDatosValidos_NoTieneErrores()
    {
        var command = new RegistrarClienteCommand(
            "CC", "1234567890", "Juan Pérez", "juan.perez@rentafacil.com", "3001234567");

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConNumeroDocumentoVacio_TieneErrorEnNumeroDocumento()
    {
        var command = new RegistrarClienteCommand(
            "CC", string.Empty, "Juan Pérez", "juan.perez@rentafacil.com", "3001234567");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.NumeroDocumento);
    }

    [Fact]
    public void Validate_ConEmailVacio_TieneErrorEnEmail()
    {
        var command = new RegistrarClienteCommand(
            "CC", "1234567890", "Juan Pérez", string.Empty, "3001234567");

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Email);
    }
}
