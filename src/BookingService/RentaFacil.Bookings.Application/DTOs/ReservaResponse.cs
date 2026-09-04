namespace RentaFacil.Bookings.Application.DTOs;

public sealed record ReservaResponse(
    Guid Id,
    Guid ClienteId,
    Guid VehiculoId,
    string PlacaVehiculo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    decimal ValorTotal);
