using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Domain.ValueObjects;

public sealed class Dinero : ValueObject
{
    private const int LongitudMoneda = 3;

    private Dinero(decimal monto, string moneda)
    {
        Monto = monto;
        Moneda = moneda;
    }

    public decimal Monto { get; }

    public string Moneda { get; }

    public static Result<Dinero> Create(decimal monto, string moneda)
    {
        if (monto < 0)
        {
            return Result.Failure<Dinero>(ReservaErrors.TarifaInvalida);
        }

        if (string.IsNullOrWhiteSpace(moneda) ||
            moneda.Length != LongitudMoneda ||
            !moneda.All(char.IsLetter))
        {
            return Result.Failure<Dinero>(ReservaErrors.TarifaInvalida);
        }

        return new Dinero(monto, moneda.ToUpperInvariant());
    }

    public Result<Dinero> Sumar(Dinero otro)
    {
        if (Moneda != otro.Moneda)
        {
            return Result.Failure<Dinero>(ReservaErrors.TarifaInvalida);
        }

        return Create(Monto + otro.Monto, Moneda);
    }

    public Dinero MultiplicarPor(int factor)
    {
        return new Dinero(Monto * factor, Moneda);
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Monto;
        yield return Moneda;
    }
}
