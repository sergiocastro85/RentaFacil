using RentaFacil.SharedKernel.Primitives;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Domain.Entities;

public sealed class Cliente : AggregateRoot
{
    private Cliente(
        Guid id,
        Documento documento,
        string nombreCompleto,
        Email email,
        string telefono,
        DateTime fechaRegistro)
        : base(id)
    {
        Documento = documento;
        NombreCompleto = nombreCompleto;
        Email = email;
        Telefono = telefono;
        FechaRegistro = fechaRegistro;
    }

    public Documento Documento { get; }

    public string NombreCompleto { get; }

    public Email Email { get; }

    public string Telefono { get; }

    public DateTime FechaRegistro { get; }

    public static Result<Cliente> Crear(
        Guid id,
        string tipoDocumento,
        string numeroDocumento,
        string nombreCompleto,
        string email,
        string telefono,
        DateTime fechaActual)
    {
        var documentoResult = Documento.Create(tipoDocumento, numeroDocumento);
        if (documentoResult.IsFailure)
        {
            return Result.Failure<Cliente>(documentoResult.Error);
        }

        var emailResult = Email.Create(email);
        if (emailResult.IsFailure)
        {
            return Result.Failure<Cliente>(emailResult.Error);
        }

        return new Cliente(id, documentoResult.Value, nombreCompleto, emailResult.Value, telefono, fechaActual);
    }
}
