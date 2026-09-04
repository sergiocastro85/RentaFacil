using RentaFacil.Bookings.Domain.Enums;

namespace RentaFacil.Bookings.Application.Abstractions;

public sealed record CupoReservadoDto(Guid BloqueoId, string Placa, TipoVehiculo Tipo, decimal TarifaDiaria);
