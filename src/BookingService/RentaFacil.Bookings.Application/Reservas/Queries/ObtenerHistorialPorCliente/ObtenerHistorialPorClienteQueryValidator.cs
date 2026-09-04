using FluentValidation;

namespace RentaFacil.Bookings.Application.Reservas.Queries.ObtenerHistorialPorCliente;

public sealed class ObtenerHistorialPorClienteQueryValidator : AbstractValidator<ObtenerHistorialPorClienteQuery>
{
    public ObtenerHistorialPorClienteQueryValidator()
    {
        RuleFor(query => query.ClienteId)
            .NotEmpty();
    }
}
