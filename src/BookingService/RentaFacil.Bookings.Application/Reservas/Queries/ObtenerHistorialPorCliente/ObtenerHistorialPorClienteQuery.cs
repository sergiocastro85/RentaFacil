using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;

namespace RentaFacil.Bookings.Application.Reservas.Queries.ObtenerHistorialPorCliente;

public sealed record ObtenerHistorialPorClienteQuery(Guid ClienteId) : IRequest<Result<IReadOnlyList<HistorialReservaDto>>>;
