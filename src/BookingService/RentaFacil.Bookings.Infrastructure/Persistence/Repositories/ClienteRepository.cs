using Microsoft.EntityFrameworkCore;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Infrastructure.Persistence.Repositories;

internal sealed class ClienteRepository : IClienteRepository
{
    private readonly BookingsDbContext _dbContext;

    public ClienteRepository(BookingsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(cliente => cliente.Id == id, cancellationToken);
    }

    public async Task<Cliente?> ObtenerPorDocumentoAsync(Documento documento, CancellationToken cancellationToken)
    {
        return await _dbContext.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(cliente => cliente.Documento == documento, cancellationToken);
    }

    public async Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken)
    {
        await _dbContext.Clientes.AddAsync(cliente, cancellationToken);
    }
}
