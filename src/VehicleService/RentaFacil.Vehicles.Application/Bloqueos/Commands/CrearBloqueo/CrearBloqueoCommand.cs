using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.DTOs;

namespace RentaFacil.Vehicles.Application.Bloqueos.Commands.CrearBloqueo;

public sealed record CrearBloqueoCommand(
    Guid VehiculoId,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    Guid ReferenciaExternaId) : IRequest<Result<BloqueoResponse>>;
