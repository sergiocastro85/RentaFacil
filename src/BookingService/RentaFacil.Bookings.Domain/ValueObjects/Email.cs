using System.Text.RegularExpressions;
using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Domain.ValueObjects;

public sealed partial class Email : ValueObject
{
    private Email(string valor)
    {
        Valor = valor;
    }

    public string Valor { get; }

    public static Result<Email> Create(string valor)
    {
        var normalizado = valor?.Trim().ToLowerInvariant() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(normalizado) || !FormatoValido().IsMatch(normalizado))
        {
            return Result.Failure<Email>(ClienteErrors.EmailInvalido);
        }

        return new Email(normalizado);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Valor;
    }

    public override string ToString() => Valor;

    [GeneratedRegex(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")]
    private static partial Regex FormatoValido();
}
