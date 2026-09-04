using MediatR;
using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Vehicles.Application.Bloqueos.Commands.LiberarBloqueo;

public sealed record LiberarBloqueoCommand(Guid VehiculoId, Guid ReferenciaExternaId) : IRequest<Result>;
