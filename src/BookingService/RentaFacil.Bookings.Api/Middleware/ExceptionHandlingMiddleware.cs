using Microsoft.AspNetCore.Mvc;

namespace RentaFacil.Bookings.Api.Middleware;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            var traceId = context.TraceIdentifier;

            _logger.LogError(
                exception,
                "Excepción no controlada procesando la solicitud {Method} {Path}. TraceId: {TraceId}",
                context.Request.Method,
                context.Request.Path,
                traceId);

            var problemDetails = new ProblemDetails
            {
                Type = "https://rentafacil/errors/error-interno",
                Title = "Error interno del servidor",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Ocurrió un error inesperado. Contacte al administrador si el problema persiste.",
            };
            problemDetails.Extensions["traceId"] = traceId;

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
