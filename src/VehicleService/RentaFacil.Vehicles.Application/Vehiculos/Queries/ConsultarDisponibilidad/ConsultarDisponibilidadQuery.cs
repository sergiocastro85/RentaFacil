using MediatR;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Application.Vehiculos.Queries.ConsultarDisponibilidad;

public sealed record ConsultarDisponibilidadQuery(
    TipoVehiculo Tipo,
    DateOnly FechaInicio,
    DateOnly FechaFin) : IRequest<Result<IReadOnlyList<VehiculoDisponibleDto>>>;
