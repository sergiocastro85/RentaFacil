using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Domain.Repositories;

public interface IReporteRepository
{
    Task<ReporteReservasDiarias?> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancellationToken);

    Task AgregarAsync(ReporteReservasDiarias reporte, CancellationToken cancellationToken);

    // Agregación traducida a SQL (GroupBy + Count/Sum + COUNT DISTINCT): el handler de
    // GenerarReporteDiario no trae reservas completas a memoria para sumarlas con LINQ to
    // Objects.
    Task<ReporteAgregado> ObtenerAgregadoEnRangoAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken);
}
