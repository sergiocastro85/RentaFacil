using Microsoft.EntityFrameworkCore;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Repositories;

namespace RentaFacil.Bookings.Infrastructure.Persistence.Repositories;

internal sealed class ReservaRepository : IReservaRepository
{
    private readonly BookingsDbContext _dbContext;

    public ReservaRepository(BookingsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AgregarAsync(Reserva reserva, CancellationToken cancellationToken)
    {
        await _dbContext.Reservas.AddAsync(reserva, cancellationToken);
    }

    public async Task<IReadOnlyCollection<Reserva>> ObtenerHistorialPorClienteAsync(
        Guid clienteId,
        CancellationToken cancellationToken)
    {
        return await _dbContext.Reservas
            .AsNoTracking()
            .Where(reserva => reserva.ClienteId == clienteId)
            .OrderByDescending(reserva => reserva.FechaCreacion)
            .ToListAsync(cancellationToken);
    }
}
