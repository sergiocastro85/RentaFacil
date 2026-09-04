using RentaFacil.SharedKernel.Primitives;

namespace RentaFacil.Bookings.Domain.Entities;

public sealed class ReporteReservasDiarias : Entity
{
    private ReporteReservasDiarias(
        Guid id,
        DateOnly fecha,
        int totalReservas,
        decimal valorTotalReservado,
        string tipoVehiculoMasReservado,
        int clientesUnicos,
        string detalleJson,
        DateTime fechaProcesamiento)
        : base(id)
    {
        Fecha = fecha;
        TotalReservas = totalReservas;
        ValorTotalReservado = valorTotalReservado;
        TipoVehiculoMasReservado = tipoVehiculoMasReservado;
        ClientesUnicos = clientesUnicos;
        DetalleJson = detalleJson;
        FechaProcesamiento = fechaProcesamiento;
    }

    public DateOnly Fecha { get; }

    public int TotalReservas { get; private set; }

    public decimal ValorTotalReservado { get; private set; }

    public string TipoVehiculoMasReservado { get; private set; }

    public int ClientesUnicos { get; private set; }

    public string DetalleJson { get; private set; }

    public DateTime FechaProcesamiento { get; private set; }

    public static ReporteReservasDiarias Crear(
        Guid id,
        DateOnly fecha,
        int totalReservas,
        decimal valorTotalReservado,
        string tipoVehiculoMasReservado,
        int clientesUnicos,
        string detalleJson,
        DateTime fechaProcesamiento)
    {
        return new ReporteReservasDiarias(
            id,
            fecha,
            totalReservas,
            valorTotalReservado,
            tipoVehiculoMasReservado,
            clientesUnicos,
            detalleJson,
            fechaProcesamiento);
    }

    // RN-RP03: reprocesar una fecha sobrescribe el registro existente (idempotente).
    public void ActualizarAgregados(
        int totalReservas,
        decimal valorTotalReservado,
        string tipoVehiculoMasReservado,
        int clientesUnicos,
        string detalleJson,
        DateTime fechaProcesamiento)
    {
        TotalReservas = totalReservas;
        ValorTotalReservado = valorTotalReservado;
        TipoVehiculoMasReservado = tipoVehiculoMasReservado;
        ClientesUnicos = clientesUnicos;
        DetalleJson = detalleJson;
        FechaProcesamiento = fechaProcesamiento;
    }
}
