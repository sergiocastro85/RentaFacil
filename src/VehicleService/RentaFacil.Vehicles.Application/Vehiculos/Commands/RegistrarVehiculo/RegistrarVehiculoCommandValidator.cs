using FluentValidation;
using RentaFacil.SharedKernel.Abstractions;

namespace RentaFacil.Vehicles.Application.Vehiculos.Commands.RegistrarVehiculo;

public sealed class RegistrarVehiculoCommandValidator : AbstractValidator<RegistrarVehiculoCommand>
{
    public RegistrarVehiculoCommandValidator(IDateTimeProvider dateTimeProvider)
    {
        var anioMaximo = dateTimeProvider.UtcNow.Year + 1;

        RuleFor(command => command.Placa).NotEmpty();

        RuleFor(command => command.Tipo).IsInEnum();

        RuleFor(command => command.Marca)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(command => command.Modelo)
            .NotEmpty()
            .MaximumLength(60);

        RuleFor(command => command.Anio)
            .InclusiveBetween(1990, anioMaximo);

        RuleFor(command => command.TarifaDiaria)
            .GreaterThan(0);

        RuleFor(command => command.Moneda)
            .NotEmpty()
            .Length(3);
    }
}
