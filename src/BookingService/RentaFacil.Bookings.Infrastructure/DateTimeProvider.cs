using RentaFacil.SharedKernel.Abstractions;

namespace RentaFacil.Bookings.Infrastructure;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
