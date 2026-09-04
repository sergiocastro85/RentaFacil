using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Vehicles.Domain.Errors;

namespace RentaFacil.Vehicles.Domain.ValueObjects;

public sealed class Placa : ValueObject
{
    private const int LongitudMinima = 5;
    private const int LongitudMaxima = 10;

    private Placa(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Result<Placa> Create(string valor)
    {
        var normalizada = Normalizar(valor);

        if (normalizada.Length < LongitudMinima ||
            normalizada.Length > LongitudMaxima ||
            !normalizada.All(char.IsLetterOrDigit))
        {
            return Result.Failure<Placa>(VehiculoErrors.PlacaInvalida);
        }

        return new Placa(normalizada);
    }

    private static string Normalizar(string valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return string.Empty;
        }

        return valor.Replace(" ", string.Empty).ToUpperInvariant();
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;
}
