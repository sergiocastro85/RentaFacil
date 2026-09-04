using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Domain.ValueObjects;

public sealed class Documento : ValueObject
{
    private const int LongitudMinima = 5;
    private const int LongitudMaxima = 15;

    private Documento(string tipo, string numero)
    {
        Tipo = tipo;
        Numero = numero;
    }

    public string Tipo { get; }

    public string Numero { get; }

    public static Result<Documento> Create(string tipo, string numero)
    {
        if (string.IsNullOrWhiteSpace(tipo))
        {
            return Result.Failure<Documento>(ClienteErrors.DocumentoInvalido);
        }

        var numeroNormalizado = numero?.Trim() ?? string.Empty;

        if (numeroNormalizado.Length < LongitudMinima ||
            numeroNormalizado.Length > LongitudMaxima ||
            !numeroNormalizado.All(char.IsDigit))
        {
            return Result.Failure<Documento>(ClienteErrors.DocumentoInvalido);
        }

        return new Documento(tipo.Trim(), numeroNormalizado);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Tipo;
        yield return Numero;
    }
}
