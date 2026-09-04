using MediatR;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Application.Vehiculos.Commands.RegistrarVehiculo;

public sealed class RegistrarVehiculoCommandHandler
    : IRequestHandler<RegistrarVehiculoCommand, Result<VehiculoResponse>>
{
    private readonly IVehiculoRepository _vehiculoRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RegistrarVehiculoCommandHandler(
        IVehiculoRepository vehiculoRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider)
    {
        _vehiculoRepository = vehiculoRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<VehiculoResponse>> Handle(
        RegistrarVehiculoCommand request,
        CancellationToken cancellationToken)
    {
        var placaResult = Placa.Create(request.Placa);
        if (placaResult.IsFailure)
        {
            return Result.Failure<VehiculoResponse>(placaResult.Error);
        }

        var vehiculoExistente = await _vehiculoRepository.ObtenerPorPlacaAsync(placaResult.Value, cancellationToken);
        if (vehiculoExistente is not null)
        {
            return Result.Failure<VehiculoResponse>(VehiculoErrors.PlacaDuplicada(placaResult.Value.Valor));
        }

        var dineroResult = Dinero.Create(request.TarifaDiaria, request.Moneda);
        if (dineroResult.IsFailure)
        {
            return Result.Failure<VehiculoResponse>(dineroResult.Error);
        }

        var fechaActual = _dateTimeProvider.UtcNow;

        var vehiculoResult = Vehiculo.Crear(
            Guid.NewGuid(),
            placaResult.Value,
            request.Tipo,
            request.Marca,
            request.Modelo,
            request.Anio,
            dineroResult.Value,
            fechaActual);

        if (vehiculoResult.IsFailure)
        {
            return Result.Failure<VehiculoResponse>(vehiculoResult.Error);
        }

        var vehiculo = vehiculoResult.Value;

        await _vehiculoRepository.AgregarAsync(vehiculo, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new VehiculoResponse(
            vehiculo.Id,
            vehiculo.Placa.Valor,
            vehiculo.Tipo,
            vehiculo.Marca,
            vehiculo.Modelo,
            vehiculo.Anio,
            vehiculo.TarifaDiaria.Monto);
    }
}
