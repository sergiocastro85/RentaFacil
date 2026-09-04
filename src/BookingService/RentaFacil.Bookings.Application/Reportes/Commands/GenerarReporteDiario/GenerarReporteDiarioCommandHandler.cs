using System.Diagnostics;
using System.Text.Json;
using MediatR;
using Microsoft.Extensions.Logging;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.DTOs;
using RentaFacil.Bookings.Domain.Entities;
using RentaFacil.Bookings.Domain.Repositories;

namespace RentaFacil.Bookings.Application.Reportes.Commands.GenerarReporteDiario;

public sealed class GenerarReporteDiarioCommandHandler
    : IRequestHandler<GenerarReporteDiarioCommand, Result<ReporteDiarioDto>>
{
    private readonly IReporteRepository _reporteRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ILogger<GenerarReporteDiarioCommandHandler> _logger;

    public GenerarReporteDiarioCommandHandler(
        IReporteRepository reporteRepository,
        IUnitOfWork unitOfWork,
        IDateTimeProvider dateTimeProvider,
        ILogger<GenerarReporteDiarioCommandHandler> logger)
    {
        _reporteRepository = reporteRepository;
        _unitOfWork = unitOfWork;
        _dateTimeProvider = dateTimeProvider;
        _logger = logger;
    }

    public async Task<Result<ReporteDiarioDto>> Handle(
        GenerarReporteDiarioCommand request,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();

        // RN-RP02: el reporte del día D agrega las reservas con FechaCreacion en
        // [D 00:00 UTC, D+1 00:00 UTC).
        var desde = request.Fecha.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
        var hasta = request.Fecha.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);

        var agregado = await _reporteRepository.ObtenerAgregadoEnRangoAsync(desde, hasta, cancellationToken);

        var totalReservas = agregado.DesglosePorTipo.Sum(desglose => desglose.CantidadReservas);
        var valorTotalReservado = agregado.DesglosePorTipo.Sum(desglose => desglose.ValorTotal);

        var tipoMasReservado = agregado.DesglosePorTipo
            .OrderByDescending(desglose => desglose.CantidadReservas)
            .Select(desglose => desglose.Tipo.ToString())
            .FirstOrDefault() ?? string.Empty;

        var detalleJson = JsonSerializer.Serialize(
            agregado.DesglosePorTipo.ToDictionary(
                desglose => desglose.Tipo.ToString(),
                desglose => desglose.CantidadReservas));

        var fechaActual = _dateTimeProvider.UtcNow;

        // RN-RP01 / RN-RP03: un solo registro por fecha; reprocesar la sobrescribe (upsert
        // idempotente).
        var reporteExistente = await _reporteRepository.ObtenerPorFechaAsync(request.Fecha, cancellationToken);

        if (reporteExistente is null)
        {
            var nuevoReporte = ReporteReservasDiarias.Crear(
                Guid.NewGuid(),
                request.Fecha,
                totalReservas,
                valorTotalReservado,
                tipoMasReservado,
                agregado.ClientesUnicos,
                detalleJson,
                fechaActual);

            await _reporteRepository.AgregarAsync(nuevoReporte, cancellationToken);
        }
        else
        {
            reporteExistente.ActualizarAgregados(
                totalReservas,
                valorTotalReservado,
                tipoMasReservado,
                agregado.ClientesUnicos,
                detalleJson,
                fechaActual);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        stopwatch.Stop();

        _logger.LogInformation(
            "Reporte diario generado. Fecha: {Fecha}, TotalReservas: {TotalReservas}, DuracionMs: {DuracionMs}",
            request.Fecha,
            totalReservas,
            stopwatch.ElapsedMilliseconds);

        return new ReporteDiarioDto(
            request.Fecha,
            totalReservas,
            valorTotalReservado,
            tipoMasReservado,
            agregado.ClientesUnicos,
            detalleJson,
            fechaActual);
    }
}
