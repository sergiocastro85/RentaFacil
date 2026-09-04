namespace RentaFacil.Bookings.Application.DTOs;

public sealed record ClienteResponse(
    Guid Id,
    string Documento,
    string NombreCompleto,
    string Email,
    string Telefono);
