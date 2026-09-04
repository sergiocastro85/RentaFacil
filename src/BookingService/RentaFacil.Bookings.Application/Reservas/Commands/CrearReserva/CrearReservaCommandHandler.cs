using MediatR;
using Microsoft.Extensions.Logging;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.Abstractions;
using RentaFacil.Bookings.Application.DTOs;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Application.Reservas.Commands.CrearReserva;

public sealed class CrearReservaCommandHandler : IRequestHandler<CrearReservaCommand, Result<ReservaResponse>>
{
    private readonly IClienteRepository _clienteRepository;
    private readonly IReservaRepository _reservaRepository;
    private readonly IVehicleCatalogService _vehicleCatalogService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<CrearReservaCommandHandler> _logger;

    public CrearReservaCommandHandler(
        IClienteRepository clienteRepository,
        IReservaRepository reservaRepository,
        IVehicleCatalogService vehicleCatalogService,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<CrearReservaCommandHandler> logger)
    {
        _clienteRepository = clienteRepository;
        _reservaRepository = reservaRepository;
        _vehicleCatalogService = vehicleCatalogService;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<ReservaResponse>> Handle(CrearReservaCommand request, CancellationToken cancellationToken)
    {
        // Paso 2 (§7.2): RN-R02, el cliente debe existir antes de crear la reserva.
        var cliente = await _clienteRepository.ObtenerPorIdAsync(request.ClienteId, cancellationToken);
        if (cliente is null)
        {
            _logger.LogWarning(
                "No se puede crear la reserva: el cliente {ClienteId} no existe.",
                request.ClienteId);
            return Result.Failure<ReservaResponse>(ReservaErrors.ClienteNoEncontrado(request.ClienteId));
        }

        // Paso 3: construir el Periodo del dominio; propagar su Result si es inválido.
        var fechaActual = _dateTimeProvider.UtcNow;
        var hoy = DateOnly.FromDateTime(fechaActual);

        var periodoResult = Periodo.Create(request.FechaInicio, request.FechaFin, hoy);
        if (periodoResult.IsFailure)
        {
            return Result.Failure<ReservaResponse>(periodoResult.Error);
        }

        var periodo = periodoResult.Value;

        // Paso 4: el id de la reserva se genera antes de la llamada HTTP; es el
        // referenciaExternaId que da idempotencia al bloqueo en VehicleService (§7.3).
        var reservaId = Guid.NewGuid();

        // Paso 5: reservar el cupo en VehicleService. Si falla, se propaga tal cual y no
        // se crea la reserva local.
        var cupoResult = await _vehicleCatalogService.ReservarCupoAsync(
            request.VehiculoId,
            periodo,
            reservaId,
            cancellationToken);

        if (cupoResult.IsFailure)
        {
            _logger.LogWarning(
                "No se pudo reservar el cupo del vehículo {VehiculoId}: {ErrorCode}.",
                request.VehiculoId,
                cupoResult.Error.Code);
            return Result.Failure<ReservaResponse>(cupoResult.Error);
        }

        var cupo = cupoResult.Value;

        // Paso 6: crear la Reserva con el snapshot del cupo (placa, tipo, tarifa) y persistirla.
        var reservaResult = Reserva.Crear(
            reservaId,
            request.ClienteId,
            request.VehiculoId,
            cupo.Tipo,
            cupo.Placa,
            request.FechaInicio,
            request.FechaFin,
            cupo.TarifaDiaria,
            cupo.Moneda,
            fechaActual);

        if (reservaResult.IsFailure)
        {
            await CompensarAsync(request.VehiculoId, reservaId, cancellationToken);
            return Result.Failure<ReservaResponse>(reservaResult.Error);
        }

        var reserva = reservaResult.Value;

        try
        {
            await _reservaRepository.AgregarAsync(reserva, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (Exception exception)
        {
            // Paso 7: compensación. El fallo de la compensación se loguea pero no cambia
            // el resultado devuelto al llamador.
            _logger.LogError(
                exception,
                "Fallo al persistir la reserva {ReservaId} del vehículo {VehiculoId}. Iniciando compensación.",
                reservaId,
                request.VehiculoId);

            await CompensarAsync(request.VehiculoId, reservaId, cancellationToken);

            return Result.Failure<ReservaResponse>(ReservaErrors.FalloAlPersistir);
        }

        _logger.LogInformation(
            "Reserva {ReservaId} creada para el cliente {ClienteId} y el vehículo {VehiculoId}.",
            reservaId,
            request.ClienteId,
            request.VehiculoId);

        return new ReservaResponse(
            reserva.Id,
            reserva.ClienteId,
            reserva.VehiculoId,
            reserva.PlacaVehiculo,
            reserva.Periodo.FechaInicio,
            reserva.Periodo.FechaFin,
            reserva.ValorTotal.Monto);
    }

    private async Task CompensarAsync(Guid vehiculoId, Guid referenciaExternaId, CancellationToken cancellationToken)
    {
        var liberarResult = await _vehicleCatalogService.LiberarCupoAsync(vehiculoId, referenciaExternaId, cancellationToken);

        if (liberarResult.IsFailure)
        {
            _logger.LogError(
                "Falló la compensación: no se pudo liberar el cupo del vehículo {VehiculoId} (referencia {ReferenciaExternaId}). {ErrorCode}",
                vehiculoId,
                referenciaExternaId,
                liberarResult.Error.Code);
        }
    }
}
