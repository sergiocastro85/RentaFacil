using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Application.Abstractions;

public interface IVehicleCatalogService
{
    Task<Result<CupoReservadoDto>> ReservarCupoAsync(
        Guid vehiculoId,
        Periodo periodo,
        Guid referenciaExternaId,
        CancellationToken cancellationToken);

    Task<Result> LiberarCupoAsync(
        Guid vehiculoId,
        Guid referenciaExternaId,
        CancellationToken cancellationToken);
}
