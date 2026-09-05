using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Vehicles.Application.Behaviors;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        _logger.LogInformation("Iniciando solicitud {RequestName}", requestName);

        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (response is Result { IsFailure: true } resultadoFallido)
        {
            _logger.LogWarning(
                "Solicitud {RequestName} finalizada con error en {ElapsedMilliseconds} ms: {ErrorCode}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                resultadoFallido.Error.Code);
        }
        else
        {
            _logger.LogInformation(
                "Solicitud {RequestName} finalizada en {ElapsedMilliseconds} ms",
                requestName,
                stopwatch.ElapsedMilliseconds);
        }

        return response;
    }
}
