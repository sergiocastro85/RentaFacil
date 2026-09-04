using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using RentaFacil.SharedKernel.Results;
using RentaFacil.Bookings.Application.Abstractions;
using RentaFacil.Bookings.Domain.Enums;
using RentaFacil.Bookings.Domain.Errors;
using RentaFacil.Bookings.Domain.ValueObjects;

namespace RentaFacil.Bookings.Infrastructure.Http;

internal sealed class VehicleCatalogHttpClient : IVehicleCatalogService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly HttpClient _httpClient;
    private readonly ILogger<VehicleCatalogHttpClient> _logger;

    public VehicleCatalogHttpClient(HttpClient httpClient, ILogger<VehicleCatalogHttpClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<Result<CupoReservadoDto>> ReservarCupoAsync(
        Guid vehiculoId,
        Periodo periodo,
        Guid referenciaExternaId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/vehiculos/{vehiculoId}/bloqueos";
        var body = new CrearBloqueoHttpRequest(periodo.FechaInicio, periodo.FechaFin, referenciaExternaId);

        var respuesta = await EnviarAsync(
            HttpMethod.Post,
            requestUri,
            cancellationToken,
            () => _httpClient.PostAsJsonAsync(requestUri, body, JsonOptions, cancellationToken));

        if (respuesta.IsFailure)
        {
            return Result.Failure<CupoReservadoDto>(respuesta.Error);
        }

        using var response = respuesta.Value;

        if (response.IsSuccessStatusCode)
        {
            var bloqueo = await response.Content.ReadFromJsonAsync<BloqueoHttpResponse>(JsonOptions, cancellationToken);
            return new CupoReservadoDto(bloqueo!.BloqueoId, bloqueo.Placa, bloqueo.Tipo, bloqueo.TarifaDiaria, bloqueo.Moneda);
        }

        return Result.Failure<CupoReservadoDto>(TraducirError(response.StatusCode, vehiculoId));
    }

    public async Task<Result> LiberarCupoAsync(
        Guid vehiculoId,
        Guid referenciaExternaId,
        CancellationToken cancellationToken)
    {
        var requestUri = $"api/vehiculos/{vehiculoId}/bloqueos/{referenciaExternaId}";

        var respuesta = await EnviarAsync(
            HttpMethod.Delete,
            requestUri,
            cancellationToken,
            () => _httpClient.DeleteAsync(requestUri, cancellationToken));

        if (respuesta.IsFailure)
        {
            return Result.Failure(respuesta.Error);
        }

        using var response = respuesta.Value;

        return response.IsSuccessStatusCode
            ? Result.Success()
            : Result.Failure(TraducirError(response.StatusCode, vehiculoId));
    }

    private async Task<Result<HttpResponseMessage>> EnviarAsync(
        HttpMethod method,
        string requestUri,
        CancellationToken cancellationToken,
        Func<Task<HttpResponseMessage>> enviar)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await enviar();
            stopwatch.Stop();

            _logger.LogInformation(
                "Llamada a VehicleService completada. Método: {Metodo}, Url: {Url}, StatusCode: {StatusCode}, DuraciónMs: {DuracionMs}",
                method,
                requestUri,
                (int)response.StatusCode,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception exception)
        {
            stopwatch.Stop();

            _logger.LogWarning(
                exception,
                "Fallo de comunicación con VehicleService. Método: {Metodo}, Url: {Url}, DuraciónMs: {DuracionMs}",
                method,
                requestUri,
                stopwatch.ElapsedMilliseconds);

            return Result.Failure<HttpResponseMessage>(ReservaErrors.FalloComunicacionVehicleService);
        }
    }

    private static Error TraducirError(HttpStatusCode statusCode, Guid vehiculoId) => statusCode switch
    {
        HttpStatusCode.Conflict => ReservaErrors.VehiculoNoDisponible(vehiculoId),
        HttpStatusCode.NotFound => ReservaErrors.VehiculoNoEncontrado(vehiculoId),
        _ => ReservaErrors.FalloComunicacionVehicleService
    };

    private sealed record CrearBloqueoHttpRequest(DateOnly FechaInicio, DateOnly FechaFin, Guid ReferenciaExternaId);

    private sealed record BloqueoHttpResponse(
        Guid BloqueoId,
        Guid VehiculoId,
        string Placa,
        TipoVehiculo Tipo,
        decimal TarifaDiaria,
        string Moneda,
        DateOnly FechaInicio,
        DateOnly FechaFin);
}
