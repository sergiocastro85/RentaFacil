using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Bookings.Application.Abstractions;
using RentaFacil.Bookings.Domain.Repositories;
using RentaFacil.Bookings.Infrastructure.Http;
using RentaFacil.Bookings.Infrastructure.Persistence;
using RentaFacil.Bookings.Infrastructure.Persistence.Repositories;

namespace RentaFacil.Bookings.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<BookingsDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Bookings"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BookingsDbContext>());
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IReservaRepository, ReservaRepository>();
        services.AddScoped<IReporteRepository, ReporteRepository>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddHttpClient<IVehicleCatalogService, VehicleCatalogHttpClient>(client =>
            {
                var baseUrl = configuration["VehicleService:BaseUrl"]
                    ?? throw new InvalidOperationException("Falta la configuración 'VehicleService:BaseUrl'.");

                client.BaseAddress = new Uri(baseUrl);
            })
            .AddPolicyHandler(GetTimeoutPolicy())
            .AddPolicyHandler(GetRetryPolicy());

        return services;
    }

    // Timeout innermost: cada intento (incluidos los reintentos) tiene su propio límite de 5s.
    private static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy()
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(5));
    }

    // Retry outermost: reintenta ante fallos de red, 5xx o timeout. Nunca ante 4xx (§7.3).
    // Un solo reintento ("reintento simple"); es seguro porque el bloqueo es idempotente
    // por ReferenciaExternaId.
    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(1, _ => TimeSpan.FromMilliseconds(500));
    }
}
