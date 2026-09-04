using MediatR;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.Bloqueos.Commands.CrearBloqueo;

public sealed class CrearBloqueoCommandHandler : IRequestHandler<CrearBloqueoCommand, Result<BloqueoResponse>>
{
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public CrearBloqueoCommandHandler(
        IVehiculoRepository vehiculoRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _vehiculoRepository = vehiculoRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public Task<Result<BloqueoResponse>> Handle(CrearBloqueoCommand request, CancellationToken cancellationToken)
    {
        // ARCHITECTURE.md §7.2 y §11 decisión 10: la sección crítica (verificar solapamiento
        // e insertar el bloqueo) corre en una transacción Serializable para evitar doble reserva.
        return _unitOfWork.ExecuteInTransactionAsync(
            ct => HandleInternalAsync(request, ct),
            cancellationToken);
    }

    private async Task<Result<BloqueoResponse>> HandleInternalAsync(
        CrearBloqueoCommand request,
        CancellationToken cancellationToken)
    {
        var vehiculo = await _vehiculoRepository.ObtenerPorIdAsync(request.VehiculoId, cancellationToken);
        if (vehiculo is null)
        {
            return Result.Failure<BloqueoResponse>(VehiculoErrors.VehiculoNoEncontrado(request.VehiculoId));
        }

        var bloqueoExistente = vehiculo.Bloqueos.FirstOrDefault(
            bloqueo => bloqueo.ReferenciaExternaId == request.ReferenciaExternaId);

        if (bloqueoExistente is not null &&
            bloqueoExistente.Periodo.FechaInicio == request.FechaInicio &&
            bloqueoExistente.Periodo.FechaFin == request.FechaFin)
        {
            return CrearRespuesta(vehiculo, bloqueoExistente);
        }

        var fechaActual = _dateTimeProvider.UtcNow;
        var hoy = DateOnly.FromDateTime(fechaActual);

        var periodoResult = Periodo.Create(request.FechaInicio, request.FechaFin, hoy);
        if (periodoResult.IsFailure)
        {
            return Result.Failure<BloqueoResponse>(periodoResult.Error);
        }

        var agregarResult = vehiculo.AgregarBloqueo(periodoResult.Value, request.ReferenciaExternaId, fechaActual);
        if (agregarResult.IsFailure)
        {
            return Result.Failure<BloqueoResponse>(agregarResult.Error);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var bloqueoCreado = vehiculo.Bloqueos.Single(
            bloqueo => bloqueo.ReferenciaExternaId == request.ReferenciaExternaId);

        return CrearRespuesta(vehiculo, bloqueoCreado);
    }

    private static BloqueoResponse CrearRespuesta(Vehiculo vehiculo, BloqueoDisponibilidad bloqueo)
    {
        return new BloqueoResponse(
            bloqueo.Id,
            vehiculo.Id,
            vehiculo.Placa.Valor,
            vehiculo.Tipo,
            vehiculo.TarifaDiaria.Monto,
            vehiculo.TarifaDiaria.Moneda,
            bloqueo.Periodo.FechaInicio,
            bloqueo.Periodo.FechaFin);
    }
}
