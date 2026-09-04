namespace RentaFacil.Bookings.Domain.Repositories;

public sealed record ReporteAgregado(int ClientesUnicos, IReadOnlyCollection<DesgloseTipoVehiculo> DesglosePorTipo);
