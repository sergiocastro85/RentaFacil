using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentaFacil.Bookings.Api.Extensions;
using RentaFacil.Bookings.Application.Clientes.Commands.RegistrarCliente;
using RentaFacil.Bookings.Application.DTOs;
using RentaFacil.Bookings.Application.Reservas.Queries.ObtenerHistorialPorCliente;

namespace RentaFacil.Bookings.Api.Controllers;

[ApiController]
[Route("api/clientes")]
public sealed class ClientesController : ControllerBase
{
    private readonly ISender _sender;

    public ClientesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(ClienteResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarCliente(
        RegistrarClienteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Created($"/api/clientes/{result.Value.Id}", result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("{clienteId:guid}/reservas")]
    [ProducesResponseType(typeof(IReadOnlyList<HistorialReservaDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObtenerHistorial(Guid clienteId, CancellationToken cancellationToken)
    {
        var query = new ObtenerHistorialPorClienteQuery(clienteId);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }
}
