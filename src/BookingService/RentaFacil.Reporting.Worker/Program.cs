using RentaFacil.Bookings.Application;
using RentaFacil.Bookings.Infrastructure;
using RentaFacil.Reporting.Worker;
using RentaFacil.Reporting.Worker.Workers;
using Serilog;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddSerilog((_, loggerConfiguration) => loggerConfiguration
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("logs/worker-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.Configure<ReportingWorkerOptions>(
    builder.Configuration.GetSection(ReportingWorkerOptions.SectionName));

builder.Services.AddHostedService<ReporteDiarioBackgroundService>();

var host = builder.Build();
host.Run();
