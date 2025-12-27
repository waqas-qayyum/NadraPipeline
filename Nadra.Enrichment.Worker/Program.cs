using Microsoft.Extensions.Logging.EventLog;
using Nadra.Enrichment.Worker;
using Nadra.Enrichment.Worker.Options;
using Nadra.Enrichment.Worker.Repositories;
using Nadra.Enrichment.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);


builder.Services.AddWindowsService(options =>
{
    options.ServiceName = "DBSS All Transactions Maria DB Enrich";
});

builder.Logging.ClearProviders();
builder.Logging.AddEventLog(new EventLogSettings
{
    SourceName = "DBSSMariaEnrich",
    LogName = "Application"
});
builder.Logging.AddConsole();






builder.Services.Configure<EnrichmentOptions>(
    builder.Configuration.GetSection("Enrichment"));

builder.Services.AddSingleton(
    new TrackerRepository(
        builder.Configuration.GetConnectionString("DbssDbProd")));

builder.Services.AddSingleton(
    new CitizenLookupRepository(
        builder.Configuration.GetConnectionString("CitizenDb")));

builder.Services.AddSingleton<NadraPayloadBuilder>();

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
