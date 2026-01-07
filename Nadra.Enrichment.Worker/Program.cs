using Microsoft.Extensions.Logging.EventLog;
using Serilog;
using Serilog.Events;
using Nadra.Enrichment.Worker;
using Nadra.Enrichment.Worker.Options;
using Nadra.Enrichment.Worker.Repositories;
using Nadra.Enrichment.Worker.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------
// Configure Serilog FIRST
// ------------------------------------------------------
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("System", LogEventLevel.Warning)
    .CreateLogger();

builder.Host.UseSerilog(Log.Logger);

// ------------------------------------------------------
// Windows Service
// ------------------------------------------------------
builder.Host.UseWindowsService(options =>
{
    options.ServiceName = "DBSS All Transactions Maria DB Enrich";
});

// ------------------------------------------------------
// Event Log (Windows)
// ------------------------------------------------------
builder.Logging.ClearProviders();
builder.Logging.AddEventLog(new EventLogSettings
{
    SourceName = "DBSSMariaEnrich",
    LogName = "Application"
});




builder.Services.Configure<EnrichmentOptions>(
    builder.Configuration.GetSection("Enrichment"));

builder.Services.AddSingleton<TrackerRepository>(sp =>
{
    var logger = sp.GetRequiredService<ILogger<TrackerRepository>>();
    var connStr = builder.Configuration.GetConnectionString("DbssDbProd");

    return new TrackerRepository(logger, connStr);
});

builder.Services.AddSingleton(
    new CitizenLookupRepository(
        builder.Configuration.GetConnectionString("CitizenDb")));

builder.Services.AddSingleton<NadraPayloadBuilder>();

builder.Services.AddHostedService<Worker>();

// ------------------------------------------------------
// Build & Run
// ------------------------------------------------------
var app = builder.Build();

app.Run();