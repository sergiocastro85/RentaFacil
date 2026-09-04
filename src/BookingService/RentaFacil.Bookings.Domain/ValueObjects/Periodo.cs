using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.Errors;

namespace RentaFacil.Bookings.Domain.ValueObjects;

public sealed class Periodo : ValueObject
{
    private Periodo(DateOnly fechaInicio, DateOnly fechaFin)
    {
        FechaInicio = fechaInicio;
        FechaFin = fechaFin;
    }

    public DateOnly FechaInicio { get; }

    public DateOnly FechaFin { get; }

    public int Dias => FechaFin.DayNumber - FechaInicio.DayNumber;

    public static Result<Periodo> Create(DateOnly fechaInicio, DateOnly fechaFin, DateOnly hoy)
    {
        if (fechaFin <= fechaInicio)
        {
            return Result.Failure<Periodo>(ReservaErrors.PeriodoInvalido);
        }

        if (fechaInicio < hoy)
        {
            return Result.Failure<Periodo>(ReservaErrors.PeriodoInvalido);
        }

        return new Periodo(fechaInicio, fechaFin);
    }

    public bool SeSolapaCon(Periodo otro)
    {
        return FechaInicio < otro.FechaFin && otro.FechaInicio < FechaFin;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FechaInicio;
        yield return FechaFin;
    }
}
