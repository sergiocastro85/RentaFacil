namespace RentaFacil.Vehicles.Application.DTOs;

public sealed record BloqueoResponse(
    Guid BloqueoId,
    Guid VehiculoId,
    string Placa,
    decimal TarifaDiaria,
    DateOnly FechaInicio,
    DateOnly FechaFin);
