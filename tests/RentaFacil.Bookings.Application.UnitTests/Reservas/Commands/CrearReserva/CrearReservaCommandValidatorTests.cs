using FluentValidation.TestHelper;
using RentaFacil.Bookings.Application.Reservas.Commands.CrearReserva;

namespace RentaFacil.Bookings.Application.UnitTests.Reservas.Commands.CrearReserva;

public class CrearReservaCommandValidatorTests
{
    private readonly CrearReservaCommandValidator _validator = new();

    [Fact]
    public void Validate_ConDatosValidos_NoTieneErrores()
    {
        var command = new CrearReservaCommand(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5));

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConClienteIdVacio_TieneErrorEnClienteId()
    {
        var command = new CrearReservaCommand(
            Guid.Empty, Guid.NewGuid(), new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.ClienteId);
    }

    [Fact]
    public void Validate_ConFechaFinAnteriorAInicio_TieneErrorEnFechaFin()
    {
        var command = new CrearReservaCommand(
            Guid.NewGuid(), Guid.NewGuid(), new DateOnly(2026, 9, 5), new DateOnly(2026, 9, 1));

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.FechaFin);
    }
}
