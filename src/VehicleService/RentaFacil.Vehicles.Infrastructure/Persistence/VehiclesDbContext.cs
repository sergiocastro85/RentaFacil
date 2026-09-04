using System.Data;
using Microsoft.EntityFrameworkCore;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Vehicles.Domain.Entities;

namespace RentaFacil.Vehicles.Infrastructure.Persistence;

public sealed class VehiclesDbContext : DbContext, IUnitOfWork
{
    public VehiclesDbContext(DbContextOptions<VehiclesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Vehiculo> Vehiculos => Set<Vehiculo>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    // ARCHITECTURE.md §7.2 y §11 decisión 10: el bloqueo de disponibilidad (lectura +
    // escritura) es la sección crítica que evita doble reserva, por eso siempre corre en
    // Serializable. Se envuelve con la execution strategy porque el DbContext tiene
    // EnableRetryOnFailure(), y EF Core exige que las transacciones manuales pasen por ahí.
    async Task<TResult> IUnitOfWork.ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        var strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

            var result = await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return result;
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehiclesDbContext).Assembly);
    }
}
