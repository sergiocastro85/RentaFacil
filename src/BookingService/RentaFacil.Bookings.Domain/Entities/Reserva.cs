using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.Entities;

public sealed class Reserva : AggregateRoot
{
    // EF Core no puede enlazar por constructor una propiedad que es un owned type
    // (Periodo, TarifaDiariaAplicada, ValorTotal): "Navigations to related entities,
    // including references to owned types, cannot be bound" (mismo caso que Vehiculo
    // y BloqueoDisponibilidad en Vehicles.Domain, Fase 2). Este constructor, sin esos
    // tres parámetros, es el que EF Core usa para materializar; los owned types se
    // asignan aparte mediante el constructor de dominio de abajo o por el propio EF
    // vía el backing field.
#pragma warning disable CS8618
    private Reserva(
        Guid id,
        Guid clienteId,
        Guid vehiculoId,
        TipoVehiculo tipoVehiculo,
        string placaVehiculo,
        DateTime fechaCreacion)
        : base(id)
    {
        ClienteId = clienteId;
        VehiculoId = vehiculoId;
        TipoVehiculo = tipoVehiculo;
        PlacaVehiculo = placaVehiculo;
        FechaCreacion = fechaCreacion;
    }
#pragma warning restore CS8618

    private Reserva(
        Guid id,
        Guid clienteId,
        Guid vehiculoId,
        TipoVehiculo tipoVehiculo,
        string placaVehiculo,
        Periodo periodo,
        Dinero tarifaDiariaAplicada,
        Dinero valorTotal,
        DateTime fechaCreacion)
        : this(id, clienteId, vehiculoId, tipoVehiculo, placaVehiculo, fechaCreacion)
    {
        Periodo = periodo;
        TarifaDiariaAplicada = tarifaDiariaAplicada;
        ValorTotal = valorTotal;
    }

    public Guid ClienteId { get; }

    public Guid VehiculoId { get; }

    public TipoVehiculo TipoVehiculo { get; }

    public string PlacaVehiculo { get; }

    public Periodo Periodo { get; }

    public Dinero TarifaDiariaAplicada { get; }

    public Dinero ValorTotal { get; }

    public DateTime FechaCreacion { get; }

    public static Result<Reserva> Crear(
        Guid id,
        Guid clienteId,
        Guid vehiculoId,
        TipoVehiculo tipoVehiculo,
        string placaVehiculo,
        DateOnly fechaInicio,
        DateOnly fechaFin,
        decimal tarifaDiariaMonto,
        string tarifaDiariaMoneda,
        DateTime fechaActual)
    {
        var hoy = DateOnly.FromDateTime(fechaActual);

        var periodoResult = Periodo.Create(fechaInicio, fechaFin, hoy);
        if (periodoResult.IsFailure)
        {
            return Result.Failure<Reserva>(periodoResult.Error);
        }

        var tarifaResult = Dinero.Create(tarifaDiariaMonto, tarifaDiariaMoneda);
        if (tarifaResult.IsFailure)
        {
            return Result.Failure<Reserva>(tarifaResult.Error);
        }

        var periodo = periodoResult.Value;
        var tarifaDiaria = tarifaResult.Value;

        // RN-R03: ValorTotal = TarifaDiaria × días. Periodo ya garantiza días >= 1 (RN-P01).
        var valorTotal = tarifaDiaria.MultiplicarPor(periodo.Dias);

        return new Reserva(
            id,
            clienteId,
            vehiculoId,
            tipoVehiculo,
            placaVehiculo,
            periodo,
            tarifaDiaria,
            valorTotal,
            fechaActual);
    }
}
