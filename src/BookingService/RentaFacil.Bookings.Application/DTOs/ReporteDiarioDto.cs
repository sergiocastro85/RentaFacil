namespace RentaFacil.Bookings.Application.DTOs;

public sealed record ReporteDiarioDto(
    DateOnly Fecha,
    int TotalReservas,
    decimal ValorTotalReservado,
    string TipoVehiculoMasReservado,
    int ClientesUnicos,
    string DetalleJson,
    DateTime FechaProcesamiento);
