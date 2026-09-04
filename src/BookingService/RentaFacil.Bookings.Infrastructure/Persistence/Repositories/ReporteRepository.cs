using Microsoft.EntityFrameworkCore;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Repositories;

namespace RentaFacil.Bookings.Infrastructure.Persistence.Repositories;

internal sealed class ReporteRepository : IReporteRepository
{
    private readonly BookingsDbContext _dbContext;

    public ReporteRepository(BookingsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    // Tracked (sin AsNoTracking): el Worker (Fase 7) carga el reporte del día para
    // llamar ReporteReservasDiarias.ActualizarAgregados(...) si ya existe (RN-RP03).
    public async Task<ReporteReservasDiarias?> ObtenerPorFechaAsync(DateOnly fecha, CancellationToken cancellationToken)
    {
        return await _dbContext.ReportesReservasDiarias
            .FirstOrDefaultAsync(reporte => reporte.Fecha == fecha, cancellationToken);
    }

    public async Task AgregarAsync(ReporteReservasDiarias reporte, CancellationToken cancellationToken)
    {
        await _dbContext.ReportesReservasDiarias.AddAsync(reporte, cancellationToken);
    }

    public async Task<ReporteAgregado> ObtenerAgregadoEnRangoAsync(
        DateTime desde,
        DateTime hasta,
        CancellationToken cancellationToken)
    {
        var reservasEnRango = _dbContext.Reservas
            .AsNoTracking()
            .Where(reserva => reserva.FechaCreacion >= desde && reserva.FechaCreacion < hasta);

        var desglosePorTipo = await reservasEnRango
            .GroupBy(reserva => reserva.TipoVehiculo)
            .Select(grupo => new DesgloseTipoVehiculo(
                grupo.Key,
                grupo.Count(),
                grupo.Sum(reserva => reserva.ValorTotal.Monto)))
            .ToListAsync(cancellationToken);

        var clientesUnicos = await reservasEnRango
            .Select(reserva => reserva.ClienteId)
            .Distinct()
            .CountAsync(cancellationToken);

        return new ReporteAgregado(clientesUnicos, desglosePorTipo);
    }
}
