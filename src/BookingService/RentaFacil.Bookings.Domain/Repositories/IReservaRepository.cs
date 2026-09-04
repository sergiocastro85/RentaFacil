using RentaFacil.Bookings.Domain.Entities;

namespace RentaFacil.Bookings.Domain.Repositories;

public interface IReservaRepository
{
    Task AgregarAsync(Reserva reserva, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Reserva>> ObtenerHistorialPorClienteAsync(
        Guid clienteId,
        CancellationToken cancellationToken);
}
