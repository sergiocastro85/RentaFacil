using MediatR;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.Vehiculos.Queries.ConsultarDisponibilidad;

public sealed class ConsultarDisponibilidadQueryHandler
    : IRequestHandler<ConsultarDisponibilidadQuery, Result<IReadOnlyList<VehiculoDisponibleDto>>>
{
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IDateTimeProvider _dateTimeProvider;

    public ConsultarDisponibilidadQueryHandler(
        IVehiculoRepository vehiculoRepository,
        IDateTimeProvider dateTimeProvider)
    {
        _vehiculoRepository = vehiculoRepository;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<IReadOnlyList<VehiculoDisponibleDto>>> Handle(
        ConsultarDisponibilidadQuery request,
        CancellationToken cancellationToken)
    {
        var hoy = DateOnly.FromDateTime(_dateTimeProvider.UtcNow);

        var periodoResult = Periodo.Create(request.FechaInicio, request.FechaFin, hoy);
        if (periodoResult.IsFailure)
        {
            return Result.Failure<IReadOnlyList<VehiculoDisponibleDto>>(periodoResult.Error);
        }

        var vehiculosDisponibles = await _vehiculoRepository.ObtenerDisponiblesAsync(
            request.Tipo,
            periodoResult.Value,
            cancellationToken);

        IReadOnlyList<VehiculoDisponibleDto> dtos = vehiculosDisponibles
            .Select(vehiculo => new VehiculoDisponibleDto(
                vehiculo.Id,
                vehiculo.Placa.Valor,
                vehiculo.Tipo,
                vehiculo.Marca,
                vehiculo.Modelo,
                vehiculo.TarifaDiaria.Monto))
            .ToList();

        return Result.Success(dtos);
    }
}
