using RentaFacil.Vehicles.Domain.Entities;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.Repositories;

public interface IVehiculoRepository
{
    Task<Vehiculo?> ObtenerPorIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Vehiculo?> ObtenerPorPlacaAsync(Placa placa, CancellationToken cancellationToken);

    Task AgregarAsync(Vehiculo vehiculo, CancellationToken cancellationToken);

    Task<IReadOnlyCollection<Vehiculo>> ObtenerDisponiblesAsync(
        TipoVehiculo tipo,
        Periodo periodo,
        CancellationToken cancellationToken);
}
