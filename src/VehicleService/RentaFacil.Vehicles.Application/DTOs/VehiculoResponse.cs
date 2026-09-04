using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.DTOs;

public sealed record VehiculoResponse(
    Guid Id,
    string Placa,
    TipoVehiculo Tipo,
    string Marca,
    string Modelo,
    int Anio,
    decimal TarifaDiaria);
