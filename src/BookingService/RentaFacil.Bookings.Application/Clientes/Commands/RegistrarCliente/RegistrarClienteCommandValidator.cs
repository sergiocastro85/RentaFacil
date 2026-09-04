using FluentValidation;

namespace RentaFacil.Bookings.Application.Clientes.Commands.RegistrarCliente;

public sealed class RegistrarClienteCommandValidator : AbstractValidator<RegistrarClienteCommand>
{
    public RegistrarClienteCommandValidator()
    {
        RuleFor(command => command.TipoDocumento)
            .NotEmpty();

        RuleFor(command => command.NumeroDocumento)
            .NotEmpty();

        RuleFor(command => command.NombreCompleto)
            .NotEmpty()
            .MaximumLength(150);

        RuleFor(command => command.Email)
            .NotEmpty();

        RuleFor(command => command.Telefono)
            .NotEmpty()
            .MaximumLength(20);
    }
}
