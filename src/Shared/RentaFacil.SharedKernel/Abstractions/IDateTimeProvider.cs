namespace RentaFacil.SharedKernel.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
