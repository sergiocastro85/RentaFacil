using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RentaFacil.SharedKernel.Abstractions;
using RentaFacil.Vehicles.Domain.Repositories;
using RentaFacil.Vehicles.Infrastructure.Persistence;
using RentaFacil.Vehicles.Infrastructure.Persistence.Repositories;

namespace RentaFacil.Vehicles.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<VehiclesDbContext>(options =>
            options.UseSqlServer(
                configuration.GetConnectionString("Vehicles"),
                sqlServerOptions => sqlServerOptions.EnableRetryOnFailure()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<VehiclesDbContext>());
        services.AddScoped<IVehiculoRepository, VehiculoRepository>();
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        return services;
    }
}
