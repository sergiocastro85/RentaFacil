using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.DTOs;

public sealed record BloqueoResponse(
    Guid BloqueoId,
    Guid VehiculoId,
    string Placa,
    TipoVehiculo Tipo,
    decimal TarifaDiaria,
    string Moneda,
    DateOnly FechaInicio,
    DateOnly FechaFin);
