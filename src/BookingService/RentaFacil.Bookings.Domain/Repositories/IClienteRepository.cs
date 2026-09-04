using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.Repositories;

public interface IClienteRepository
{
    Task<Cliente?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Cliente?> ObtenerPorDocumentoAsync(Documento documento, CancellationToken cancellationToken);

    Task AgregarAsync(Cliente cliente, CancellationToken cancellationToken);
}
