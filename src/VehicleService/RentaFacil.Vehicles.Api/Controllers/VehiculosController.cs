using MediatR;
using Microsoft.AspNetCore.Mvc;
using RentaFacil.Vehicles.Api.Extensions;
using RentaFacil.Vehicles.Application.Bloqueos.Commands.CrearBloqueo;
using RentaFacil.Vehicles.Application.Bloqueos.Commands.LiberarBloqueo;
using RentaFacil.Vehicles.Application.DTOs;
using RentaFacil.Vehicles.Application.Vehiculos.Commands.RegistrarVehiculo;
using RentaFacil.Vehicles.Application.Vehiculos.Queries.ConsultarDisponibilidad;
using RentaFacil.Vehicles.Domain.Enums;

namespace RentaFacil.Vehicles.Api.Controllers;

[ApiController]
[Route("api/vehiculos")]
public sealed class VehiculosController : ControllerBase
{
    private readonly ISender _sender;

    public VehiculosController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehiculoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RegistrarVehiculo(
        RegistrarVehiculoCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Created($"/api/vehiculos/{result.Value.Id}", result.Value)
            : result.ToProblemDetails();
    }

    [HttpGet("disponibilidad")]
    [ProducesResponseType(typeof(IReadOnlyList<VehiculoDisponibleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ConsultarDisponibilidad(
        [FromQuery] TipoVehiculo tipo,
        [FromQuery] DateOnly fechaInicio,
        [FromQuery] DateOnly fechaFin,
        CancellationToken cancellationToken)
    {
        var query = new ConsultarDisponibilidadQuery(tipo, fechaInicio, fechaFin);
        var result = await _sender.Send(query, cancellationToken);

        return result.IsSuccess ? Ok(result.Value) : result.ToProblemDetails();
    }

    [HttpPost("{id:guid}/bloqueos")]
    [ProducesResponseType(typeof(BloqueoResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CrearBloqueo(
        Guid id,
        CrearBloqueoRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CrearBloqueoCommand(id, request.FechaInicio, request.FechaFin, request.ReferenciaExternaId);
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess
            ? Created($"/api/vehiculos/{id}/bloqueos/{result.Value.BloqueoId}", result.Value)
            : result.ToProblemDetails();
    }

    [HttpDelete("{id:guid}/bloqueos/{referenciaExternaId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> LiberarBloqueo(
        Guid id,
        Guid referenciaExternaId,
        CancellationToken cancellationToken)
    {
        var command = new LiberarBloqueoCommand(id, referenciaExternaId);
        var result = await _sender.Send(command, cancellationToken);

        return result.IsSuccess ? NoContent() : result.ToProblemDetails();
    }
}
