using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Vehicles.Api.Extensions;

public static partial class ResultExtensions
{
    public static ObjectResult ToProblemDetails(this Result result)
    {
        if (result.IsSuccess)
        {
            throw new InvalidOperationException("No se puede traducir a ProblemDetails un resultado exitoso.");
        }

        var statusCode = result.Error.ErrorType switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var problemDetails = new ProblemDetails
        {
            Type = $"https://rentafacil/errors/{ToSlug(result.Error.Code)}",
            Title = TituloParaTipo(result.Error.ErrorType),
            Status = statusCode,
            Detail = result.Error.Description
        };

        return new ObjectResult(problemDetails) { StatusCode = statusCode };
    }

    private static string TituloParaTipo(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Error de validación",
        ErrorType.NotFound => "Recurso no encontrado",
        ErrorType.Conflict => "Conflicto",
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
