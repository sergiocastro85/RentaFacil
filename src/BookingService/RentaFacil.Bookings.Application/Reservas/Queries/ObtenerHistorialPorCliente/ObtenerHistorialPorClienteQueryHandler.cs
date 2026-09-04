using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.Repositories;

namespace RentaFacil.Bookings.Application.Reservas.Queries.ObtenerHistorialPorCliente;

public sealed class ObtenerHistorialPorClienteQueryHandler
    : IRequestHandler<ObtenerHistorialPorClienteQuery, Result<IReadOnlyList<HistorialReservaDto>>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IReservaRepository _reservaRepository;

    public ObtenerHistorialPorClienteQueryHandler(
        IClienteRepository clienteRepository,
        IReservaRepository reservaRepository)
    {
        _clienteRepository = clienteRepository;
        _reservaRepository = reservaRepository;
    }

    public async Task<Result<IReadOnlyList<HistorialReservaDto>>> Handle(
        ObtenerHistorialPorClienteQuery request,
        CancellationToken cancellationToken)
    {
        var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente is null)
        {
            return Result.Failure<IReadOnlyList<HistorialReservaDto>>(
                ReservaErrors.ClienteNoEncontrado(request.ClienteId));
        }

        var reservas = await _reservaRepository.ObtenerHistorialPorClienteAsync(request.ClienteId, cancellationToken);

        IReadOnlyList<HistorialReservaDto> historial = reservas
            .Select(reserva => new HistorialReservaDto(
                reserva.Id,
                reserva.VehiculoId,
                reserva.PlacaVehiculo,
                reserva.TipoVehiculo,
                reserva.Periodo.FechaInicio,
                reserva.Periodo.FechaFin,
                reserva.ValorTotal.Monto,
                reserva.FechaCreacion))
            .ToList();

        return Result.Success(historial);
    }
}
