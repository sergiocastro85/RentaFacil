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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VehiclesDbContext).Assembly);
    }
}
