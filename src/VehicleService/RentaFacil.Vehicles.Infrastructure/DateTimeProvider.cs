using RentaFacil.SharedKernel.Abstractions;

namespace RentaFacil.Vehicles.Infrastructure;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
