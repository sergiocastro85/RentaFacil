using FluentValidation.TestHelper;
using RentaFacil.Bookings.Application.Reservas.Queries.ObtenerHistorialPorCliente;

namespace RentaFacil.Bookings.Application.UnitTests.Reservas.Queries.ObtenerHistorialPorCliente;

public class ObtenerHistorialPorClienteQueryValidatorTests
{
    private readonly ObtenerHistorialPorClienteQueryValidator _validator = new();

    [Fact]
    public void Validate_ConClienteIdValido_NoTieneErrores()
    {
        var query = new ObtenerHistorialPorClienteQuery(Guid.NewGuid());

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConClienteIdVacio_TieneErrorEnClienteId()
    {
        var query = new ObtenerHistorialPorClienteQuery(Guid.Empty);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.ClienteId);
    }
}
