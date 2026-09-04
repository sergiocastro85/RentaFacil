using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Bookings.Domain.Errors;

public static class ReservaErrors
{
    public static Error ClienteNoEncontrado(Guid clienteId) => new(
        "Reserva.ClienteNoEncontrado",
        $"No existe un cliente con id {clienteId}.",
        ErrorType.NotFound);

    public static readonly Error PeriodoInvalido = new(
        "Reserva.PeriodoInvalido",
        "La fecha fin debe ser posterior a la fecha inicio y la fecha inicio no puede estar en el pasado.",
        ErrorType.Validation);

    public static readonly Error TarifaInvalida = new(
        "Reserva.TarifaInvalida",
        "La tarifa diaria debe ser un monto no negativo en una moneda válida de 3 letras.",
        ErrorType.Validation);

    public static Error VehiculoNoDisponible(Guid vehiculoId) => new(
        "Reserva.VehiculoNoDisponible",
        $"El vehículo {vehiculoId} no está disponible en el periodo solicitado.",
        ErrorType.Conflict);

    public static Error VehiculoNoEncontrado(Guid vehiculoId) => new(
        "Reserva.VehiculoNoEncontrado",
        $"No existe un vehículo con id {vehiculoId} en VehicleService.",
        ErrorType.NotFound);

    public static readonly Error FalloComunicacionVehicleService = new(
        "Reserva.FalloComunicacionVehicleService",
        "No fue posible comunicarse con el servicio de vehículos.",
        ErrorType.Failure);
}
