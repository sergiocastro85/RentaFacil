using FluentValidation.TestHelper;
using RentaFacil.Vehicles.Application.Vehiculos.Queries.ConsultarDisponibilidad;
using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.UnitTests.Vehiculos.Queries.ConsultarDisponibilidad;

public class ConsultarDisponibilidadQueryValidatorTests
{
    private readonly ConsultarDisponibilidadQueryValidator _validator = new();

    [Fact]
    public void Validate_ConDatosValidos_NoTieneErrores()
    {
        var query = new ConsultarDisponibilidadQuery(TipoVehiculo.SUV, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 5));

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_ConFechaFinAnteriorAInicio_TieneErrorEnFechaFin()
    {
        var query = new ConsultarDisponibilidadQuery(TipoVehiculo.SUV, new DateOnly(2026, 9, 5), new DateOnly(2026, 9, 1));

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(q => q.FechaFin);
    }
}
