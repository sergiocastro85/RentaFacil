using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;

namespace RentaFacil.Bookings.Application.Clientes.Commands.RegistrarCliente;

public sealed record RegistrarClienteCommand(
    string TipoDocumento,
    string NumeroDocumento,
    string NombreCompleto,
    string Email,
    string Telefono) : IRequest<Result<ClienteResponse>>;
