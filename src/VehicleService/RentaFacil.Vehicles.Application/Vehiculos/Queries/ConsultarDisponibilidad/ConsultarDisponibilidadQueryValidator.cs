using FluentValidation;

namespace RentaFacil.Vehicles.Application.Vehiculos.Queries.ConsultarDisponibilidad;

public sealed class ConsultarDisponibilidadQueryValidator : AbstractValidator<ConsultarDisponibilidadQuery>
{
    public ConsultarDisponibilidadQueryValidator()
    {
        RuleFor(query => query.Tipo).IsInEnum();

        RuleFor(query => query.FechaFin)
            .GreaterThan(query => query.FechaInicio);
    }
}
