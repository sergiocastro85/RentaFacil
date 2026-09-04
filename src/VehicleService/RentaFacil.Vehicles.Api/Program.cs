using Microsoft.EntityFrameworkCore;
using RentaFacil.Vehicles.Infrastructure;
using RentaFacil.Vehicles.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<VehiclesDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
