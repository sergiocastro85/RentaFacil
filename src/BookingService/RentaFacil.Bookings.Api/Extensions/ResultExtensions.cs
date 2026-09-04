using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Api.Extensions;

public static partial class ResultExtensions
{
    public static ObjectResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("No se puede traducir a ProblemDetails un resultado exitoso.");
        }

        var statusCode = DeterminarStatusCode(result.Error);

        var problemDetails = new ProblemDetails
        {
            Type = $"https://rentafacil/errors/{ToSlug(result.Error.Code)}",
            Title = TituloParaStatusCode(statusCode),
            Status = statusCode,
            Detail = result.Error.Description
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private static int DeterminarStatusCode(Error error) => error.ErrorType switch
    {
        ErrorType.Validation => StatusCodes.Status400BadRequest,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        // Solo el fallo de comunicación con VehicleService es "service unavailable" (invita al
        // cliente a reintentar). Un fallo de persistencia local o un bug interno no lo son, y se
        // tratan como el resto de ErrorType.Failure: 500, igual que en Vehicles.Api.
        ErrorType.Failure when error.Code == ReservaErrors.FalloComunicacionVehicleService.Code
            => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status500InternalServerError
    };

    private static string TituloParaStatusCode(int statusCode) => statusCode switch
    {
        StatusCodes.Status400BadRequest => "Error de validación",
        StatusCodes.Status404NotFound => "Recurso no encontrado",
        StatusCodes.Status409Conflict => "Conflicto",
        StatusCodes.Status503ServiceUnavailable => "Servicio no disponible",
        _ => "Error interno del servidor"
    };

    private static string ToSlug(string code)
    {
        var segments = code.Split('.');
        var kebabSegments = segments.Select(segment =>
            PalabraEnMayusculas().Replace(segment, "-$1").ToLowerInvariant());

        return string.Join('-', kebabSegments);
    }

    [GeneratedRegex("(?<!^)([A-Z])")]
    private static partial Regex PalabraEnMayusculas();
}
