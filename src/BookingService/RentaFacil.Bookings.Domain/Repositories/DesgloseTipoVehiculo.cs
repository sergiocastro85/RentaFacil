using RentaFacil.Bookings.Domain.Enums;

namespace RentaFacil.Bookings.Domain.Repositories;

public sealed record DesgloseTipoVehiculo(TipoVehiculo Tipo, int CantidadReservas, decimal ValorTotal);
