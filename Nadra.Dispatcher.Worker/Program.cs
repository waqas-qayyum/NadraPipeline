using Nadra.Dispatcher.Worker;
using Nadra.Dispatcher.Worker.Options;
using Nadra.Dispatcher.Worker.Repositories;
using Nadra.Dispatcher.Worker.Services;
using Microsoft.Extensions.Hosting.WindowsServices;

var builder = Host.CreateApplicationBuilder(args);

// -------------------------------
// Windows Service Integration
// -------------------------------
if (!Environment.UserInteractive)
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "Nadra Dispacher Worker";
    });
}

// -------------------------------
// Logging (critical for diagnostics)
// -------------------------------
// Logging configuration
builder.Logging.ClearProviders();

// Console logs (debug / F5)
if (Environment.UserInteractive)
{
    builder.Logging.AddConsole();
}

// Windows Event Log (service)
builder.Logging.AddEventLog(settings =>
{
    settings.SourceName = "Nadra.Dispacher.Worker";
    settings.LogName = "Application";
});

// -------------------------------
// Configuration
// -------------------------------
builder.Services.Configure<DispatcherOptions>(
    builder.Configuration.GetSection("Picker"));

// -------------------------------
// Repositories (DI-safe)
// -------------------------------
builder.Services.AddScoped<TrackerRepository>();
builder.Services.AddScoped<AuditRepository>();

// -------------------------------
// Http Clients
// -------------------------------
builder.Services.AddHttpClient<NadraApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["NadraApi:BaseUrl"]!);

    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(
            builder.Configuration["NadraApi:TimeoutSeconds"]!));
});

// -------------------------------
// Core Services
// -------------------------------
builder.Services.AddScoped<DispatchCoordinator>();

// -------------------------------
// Worker
// -------------------------------
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();
