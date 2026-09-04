using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.Entities;

public sealed class Vehiculo : AggregateRoot
{
    private readonly List<BloqueoDisponibilidad> _bloqueos = [];

    // EF Core no puede enlazar por constructor una propiedad que es un owned type
    // (TarifaDiaria): "Navigations to related entities, including references to
    // owned types, cannot be bound". Este constructor, sin ese parámetro, es el
    // que EF Core usa para materializar; TarifaDiaria se asigna aparte mediante
    // el constructor de dominio de abajo o por el propio EF vía el backing field.
#pragma warning disable CS8618 // TarifaDiaria la asigna el constructor de dominio (abajo) o EF Core vía el backing field.
    private Vehiculo(
        Guid id,
        Placa placa,
        TipoVehiculo tipo,
        string marca,
        string modelo,
        int anio,
        DateTime fechaRegistro)
        : base(id)
    {
        Placa = placa;
        Tipo = tipo;
        Marca = marca;
        Modelo = modelo;
        Anio = anio;
        FechaRegistro = fechaRegistro;
    }
#pragma warning restore CS8618

    private Vehiculo(
        Guid id,
        Placa placa,
        TipoVehiculo tipo,
        string marca,
        string modelo,
        int anio,
        Dinero tarifaDiaria,
        DateTime fechaRegistro)
        : this(id, placa, tipo, marca, modelo, anio, fechaRegistro)
    {
        TarifaDiaria = tarifaDiaria;
    }

    public Placa Placa { get; }

    public TipoVehiculo Tipo { get; }

    public string Marca { get; }

    public string Modelo { get; }

    public int Anio { get; }

    public Dinero TarifaDiaria { get; }

    public DateTime FechaRegistro { get; }

    public IReadOnlyCollection<BloqueoDisponibilidad> Bloqueos => _bloqueos.AsReadOnly();

    public static Result<Vehiculo> Crear(
        Guid id,
        Placa placa,
        TipoVehiculo tipo,
        string marca,
        string modelo,
        int anio,
        Dinero tarifaDiaria,
        DateTime fechaActual)
    {
        if (anio < 1990 || anio > fechaActual.Year + 1)
        {
            return Result.Failure<Vehiculo>(VehiculoErrors.AnioFueraDeRango);
        }

        if (tarifaDiaria.Monto <= 0)
        {
            return Result.Failure<Vehiculo>(VehiculoErrors.TarifaInvalida);
        }

        return new Vehiculo(id, placa, tipo, marca, modelo, anio, tarifaDiaria, fechaActual);
    }

    public Result AgregarBloqueo(Periodo periodo, Guid referenciaExternaId, DateTime fechaActual)
    {
        var solapado = _bloqueos.Any(bloqueo => bloqueo.Periodo.SeSolapaCon(periodo));
        if (solapado)
        {
            return Result.Failure(VehiculoErrors.VehiculoNoDisponible(Placa.Valor, periodo.FechaInicio, periodo.FechaFin));
        }

        _bloqueos.Add(BloqueoDisponibilidad.Crear(Id, periodo, referenciaExternaId, fechaActual));
        return Result.Success();
    }

    public Result LiberarBloqueo(Guid referenciaExternaId)
    {
        _bloqueos.RemoveAll(bloqueo => bloqueo.ReferenciaExternaId == referenciaExternaId);
        return Result.Success();
    }
}
