using RentaFacil.Bookings.Domain.Enums;

namespace RentaFacil.Bookings.Application.DTOs;

public sealed record HistorialReservaDto(
    Guid Id,
    Guid VehiculoId,
    string PlacaVehiculo,
    TipoVehiculo TipoVehiculo,
    DateOnly FechaInicio,
    DateOnly FechaFin,
    decimal ValorTotal,
    DateTime FechaCreacion);
