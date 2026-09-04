using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.Vehiculos.Commands.RegistrarVehiculo;

public sealed record RegistrarVehiculoCommand(
    string Placa,
    TipoVehiculo Tipo,
    string Marca,
    string Modelo,
    int Anio,
    decimal TarifaDiaria,
    string Moneda) : IRequest<Result<VehiculoResponse>>;
