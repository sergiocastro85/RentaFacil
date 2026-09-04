using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Domain.Enums;
using RentaFacil.Vehicles.Domain.Errors;
using RentaFacil.Vehicles.Domain.ValueObjects;

namespace RentaFacil.Vehicles.Domain.Entities;

public sealed class Vehiculo : AggregateRoot
{
    private readonly List<BloqueoDisponibilidad> _bloqueos = [];

    private Vehiculo(
        Guid id,
        Placa placa,
        TipoVehiculo tipo,
        string marca,
        string modelo,
        int anio,
        Dinero tarifaDiaria,
        DateTime fechaRegistro)
        : base(id)
    {
        Placa = placa;
        Tipo = tipo;
        Marca = marca;
        Modelo = modelo;
        Anio = anio;
        TarifaDiaria = tarifaDiaria;
        FechaRegistro = fechaRegistro;
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
