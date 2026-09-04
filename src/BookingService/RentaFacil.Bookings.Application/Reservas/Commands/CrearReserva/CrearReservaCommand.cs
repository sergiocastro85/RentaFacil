using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;

namespace RentaFacil.Bookings.Application.Reservas.Commands.CrearReserva;

public sealed record CrearReservaCommand(
    Guid ClienteId,
    Guid VehiculoId,
    DateOnly FechaInicio,
    DateOnly FechaFin) : IRequest<Result<ReservaResponse>>;
