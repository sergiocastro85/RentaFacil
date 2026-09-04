using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Domain.Repositories;

public interface IReporteRepository
{
    Task<ReporteReservasDiarias?> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancellationToken);

    Task AgregarAsync(ReporteReservasDiarias reporte, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Reserva>> ObtenerReservasEnRangoAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken);
}
