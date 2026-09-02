namespace RentaFacil.SharedKernel.Results;

public sealed record Error(string Code, string Description, ErrorType ErrorType)
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.Failure);
}
