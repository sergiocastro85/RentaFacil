using Microsoft.EntityFrameworkCore;
using RentaFacil.Bookings.Infrastructure;
using RentaFacil.Bookings.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<BookingsDbContext>();
    await dbContext.Database.MigrateAsync();
}

app.Run();
