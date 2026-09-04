using Microsoft.EntityFrameworkCore;
using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Infrastructure.Persistence.Repositories;

internal sealed class VehiculoRepository : IVehiculoRepository
{
    private readonly VehiclesDbContext _dbContext;

    public VehiculoRepository(VehiclesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Vehiculo?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Vehiculos
            .Include(vehiculo => vehiculo.Bloqueos)
            .FirstOrDefaultAsync(vehiculo => vehiculo.Id == id, cancellationToken);
    }

    public async Task<Vehiculo?> ObtenerPorPlacaAsync(Placa placa, CancellationToken cancellationToken)
    {
        return await _dbContext.Vehiculos
            .AsNoTracking()
            .FirstOrDefaultAsync(vehiculo => vehiculo.Placa == placa, cancellationToken);
    }

    public async Task AgregarAsync(Vehiculo vehiculo, CancellationToken cancellationToken)
    {
        await _dbContext.Vehiculos.AddAsync(vehiculo, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Vehiculo>> ObtenerDisponiblesAsync(
        TipoVehiculo tipo,
        Periodo periodo,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Vehiculos
            .AsNoTracking()
            .Where(vehiculo => vehiculo.Tipo == tipo)
            .Where(vehiculo => !vehiculo.Bloqueos.Any(bloqueo =>
                bloqueo.Periodo.FechaInicio < periodo.FechaFin && periodo.FechaInicio < bloqueo.Periodo.FechaFin))
            .ToListAsync(cancellationToken);
    }
}
