using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Vehicles.Domain.Errors;

public static class VehiculoErrors
{
    public static readonly Error PlacaInvalida = new(
        "Vehiculo.PlacaInvalida",
        "La placa no tiene un formato válido.",
        ErrorType.Validation);

    public static readonly Error AnioFueraDeRango = new(
        "Vehiculo.AnioFueraDeRango",
        "El año del vehículo debe estar entre 1990 y el año actual más uno.",
        ErrorType.Validation);

    public static readonly Error TarifaInvalida = new(
        "Vehiculo.TarifaInvalida",
        "La tarifa diaria debe ser un monto mayor a cero en una moneda válida de 3 letras.",
        ErrorType.Validation);

    public static readonly Error PeriodoInvalido = new(
        "Vehiculo.PeriodoInvalido",
        "La fecha fin debe ser posterior a la fecha inicio y la fecha inicio no puede estar en el pasado.",
        ErrorType.Validation);

    public static Error VehiculoNoDisponible(string placa, DateOnly fechaInicio, DateOnly fechaFin) => new(
        "Vehiculo.NoDisponible",
        $"El vehículo {placa} ya tiene una reserva entre {fechaInicio:yyyy-MM-dd} y {fechaFin:yyyy-MM-dd}.",
        ErrorType.Conflict);

    public static Error PlacaDuplicada(string placa) => new(
        "Vehiculo.PlacaDuplicada",
        $"Ya existe un vehículo registrado con la placa {placa}.",
        ErrorType.Conflict);

    public static Error VehiculoNoEncontrado(Guid vehiculoId) => new(
        "Vehiculo.NoEncontrado",
        $"No existe un vehículo con id {vehiculoId}.",
        ErrorType.NotFound);

    public static Error BloqueoNoEncontrado(Guid referenciaExternaId) => new(
        "Vehiculo.BloqueoNoEncontrado",
        $"No existe un bloqueo con referencia externa {referenciaExternaId}.",
        ErrorType.NotFound);
}
