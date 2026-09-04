using Microsoft.EntityFrameworkCore;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Infrastructure.Persistence;

public sealed class BookingsDbContext : DbContext, IUnitOfWork
{
    public BookingsDbContext(DbContextOptions<BookingsDbContext> options)
        : base(options)
    {
    }

    public DbSet<Cliente> Clientes => Set<Cliente>();

    public DbSet<Reserva> Reservas => Set<Reserva>();

    public DbSet<ReporteReservasDiarias> ReportesReservasDiarias => Set<ReporteReservasDiarias>();

    async Task IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        await SaveChangesAsync(cancellationToken);
    }

    async Task<TResult> IUnitOfWork.ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken)
    {
        var strategy = Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await Database.BeginTransactionAsync(cancellationToken);

            var result = await operation(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return result;
        });
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookingsDbContext).Assembly);
    }
}
