using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentaFacil.Bookings.Api.Extensions;
using RentaFacil.Bookings.Application.DTOs;
using RentaFacil.Bookings.Application.Reservas.Commands.CrearReserva;

namespace RentaFacil.Bookings.Api.Controllers;

[ApiController]
[Route("api/reservas")]
public sealed class ReservasController : ControllerBase
{
    private readonly ISender _sender;

    public ReservasController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ReservaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> CrearReserva(CrearReservaCommand command, CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Created($"/api/reservas/{result.Value.Id}", result.Value)
            : result.ToProblemDetails();
    }
}
