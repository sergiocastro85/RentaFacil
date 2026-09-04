using Cronos;
using MediatR;
using Microsoft.Extensions.Options;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Bookings.Application.Reportes.Commands.GenerarReporteDiario;

namespace RentaFacil.Reporting.Worker.Workers;

public sealed class ReporteDiarioBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ReportingWorkerOptions _options;
    private readonly ILogger<ReporteDiarioBackgroundService> _logger;
    private readonly CronExpression _cronExpression;

    public ReporteDiarioBackgroundService(
        IServiceScopeFactory scopeFactory,
        IDateTimeProvider dateTimeProvider,
        IOptions<ReportingWorkerOptions> options,
        ILogger<ReporteDiarioBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _dateTimeProvider = dateTimeProvider;
        _options = options.Value;
        _logger = logger;
        _cronExpression = CronExpression.Parse(_options.CronExpression);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_options.EjecutarAlIniciar)
        {
            await GenerarReporteAsync(DateOnly.FromDateTime(_dateTimeProvider.UtcNow), stoppingToken);
        }

        while (!stoppingToken.IsCancellationRequested)
        {
            var ahora = _dateTimeProvider.UtcNow;
            var proximaEjecucion = _cronExpression.GetNextOccurrence(ahora, TimeZoneInfo.Utc);

            if (proximaEjecucion is null)
            {
                _logger.LogWarning(
                    "La expresión CRON {CronExpression} no tiene una próxima ejecución. Deteniendo el scheduler.",
                    _options.CronExpression);
                return;
            }

            var espera = proximaEjecucion.Value - ahora;

            if (espera > TimeSpan.Zero)
            {
                try
                {
                    await Task.Delay(espera, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
            }

            if (stoppingToken.IsCancellationRequested)
            {
                break;
            }

            await GenerarReporteAsync(DateOnly.FromDateTime(_dateTimeProvider.UtcNow), stoppingToken);
        }
    }

    private async Task GenerarReporteAsync(DateOnly fecha, CancellationToken cancellationToken)
    {
        // Un fallo aquí nunca debe tumbar el host: se loguea y el bucle continúa.
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var sender = scope.ServiceProvider.GetRequiredService<ISender>();

            var resultado = await sender.Send(new GenerarReporteDiarioCommand(fecha), cancellationToken);

            if (resultado.IsFailure)
            {
                _logger.LogError(
                    "No se pudo generar el reporte diario de {Fecha}: {ErrorCode}",
                    fecha,
                    resultado.Error.Code);
            }
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Fallo inesperado generando el reporte diario de {Fecha}.", fecha);
        }
    }
}
