using RentaFacil.SharedKernel.Primitives;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.Entities;

public sealed class BloqueoDisponibilidad : Entity
{
    private BloqueoDisponibilidad(
        Guid id,
        Guid vehiculoId,
        Periodo periodo,
        Guid referenciaExternaId,
        DateTime fechaCreacion)
        : base(id)
    {
        VehiculoId = vehiculoId;
        Periodo = periodo;
        ReferenciaExternaId = referenciaExternaId;
        FechaCreacion = fechaCreacion;
    }

    public Guid VehiculoId { get; }

    public Periodo Periodo { get; }

    public Guid ReferenciaExternaId { get; }

    public DateTime FechaCreacion { get; }

    internal static BloqueoDisponibilidad Crear(
        Guid vehiculoId,
        Periodo periodo,
        Guid referenciaExternaId,
        DateTime fechaCreacion)
    {
        return new BloqueoDisponibilidad(Guid.NewGuid(), vehiculoId, periodo, referenciaExternaId, fechaCreacion);
    }
}
