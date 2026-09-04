using FluentValidation;

namespace RentaFacil.Bookings.Application.Reservas.Commands.CrearReserva;

public sealed class CrearReservaCommandValidator : AbstractValidator<CrearReservaCommand>
{
    public CrearReservaCommandValidator()
    {
        RuleFor(command => command.ClienteId)
            .NotEmpty();

        RuleFor(command => command.VehiculoId)
            .NotEmpty();

        RuleFor(command => command.FechaFin)
            .GreaterThan(command => command.FechaInicio);
    }
}
