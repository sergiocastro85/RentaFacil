using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.DTOs;

public sealed record VehiculoDisponibleDto(
    Guid Id,
    string Placa,
    TipoVehiculo Tipo,
    string Marca,
    string Modelo,
    decimal TarifaDiaria);
