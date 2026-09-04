using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.Entities;

public sealed class Reserva : AggregateRoot
{
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
        : base(id)
    {
        ClienteId = clienteId;
        VehiculoId = vehiculoId;
        TipoVehiculo = tipoVehiculo;
        PlacaVehiculo = placaVehiculo;
        Periodo = periodo;
        TarifaDiariaAplicada = tarifaDiariaAplicada;
        ValorTotal = valorTotal;
        FechaCreacion = fechaCreacion;
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
