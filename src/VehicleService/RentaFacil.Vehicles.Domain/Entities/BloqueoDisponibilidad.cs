using RentaFacil.SharedKernel.Primitives;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.Entities;

public sealed class BloqueoDisponibilidad : Entity
{
    // EF Core no puede enlazar por constructor una propiedad que es un owned type
    // (Periodo): "Navigations to related entities, including references to owned
    // types, cannot be bound". Este constructor, sin ese parámetro, es el que EF
    // Core usa para materializar; Periodo se asigna aparte mediante el
    // constructor de dominio de abajo o por el propio EF vía el backing field.
#pragma warning disable CS8618 // Periodo lo asigna el constructor de dominio (abajo) o EF Core vía el backing field.
    private BloqueoDisponibilidad(
        Guid id,
        Guid vehiculoId,
        Guid referenciaExternaId,
        DateTime fechaCreacion)
        : base(id)
    {
        VehiculoId = vehiculoId;
        ReferenciaExternaId = referenciaExternaId;
        FechaCreacion = fechaCreacion;
    }
#pragma warning restore CS8618

    private BloqueoDisponibilidad(
        Guid id,
        Guid vehiculoId,
        Periodo periodo,
        Guid referenciaExternaId,
        DateTime fechaCreacion)
        : this(id, vehiculoId, referenciaExternaId, fechaCreacion)
    {
        Periodo = periodo;
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
