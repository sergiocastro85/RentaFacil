using RentaFacil.SharedKernel.Results;

namespace RentaFacil.Bookings.Domain.Errors;

public static class ClienteErrors
{
    public static readonly Error DocumentoInvalido = new(
        "Cliente.DocumentoInvalido",
        "El documento no tiene un formato válido.",
        ErrorType.Validation);

    public static Error DocumentoDuplicado(string numeroDocumento) => new(
        "Cliente.DocumentoDuplicado",
        $"Ya existe un cliente registrado con el documento {numeroDocumento}.",
        ErrorType.Conflict);

    public static readonly Error EmailInvalido = new(
        "Cliente.EmailInvalido",
        "El email no tiene un formato válido.",
        ErrorType.Validation);
}
