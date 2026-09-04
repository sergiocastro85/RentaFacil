namespace RentaFacil.Vehicles.Api.Controllers;

public sealed record CrearBloqueoRequest(DateOnly FechaInicio, DateOnly FechaFin, Guid ReferenciaExternaId);
