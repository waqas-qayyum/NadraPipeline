using Nadra.Picker.Worker;
using Nadra.Picker.Worker.Options;
using Nadra.Picker.Worker.Repositories;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Hosting.WindowsServices;



var builder = Host.CreateApplicationBuilder(args);

#region EnableLogs
// Enable Windows Service integration
if (!Environment.UserInteractive)
{
    builder.Services.AddWindowsService(options =>
    {
        options.ServiceName = "Nadra Picker Worker";
    });
}

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
    settings.SourceName = "Nadra.Picker.Worker";
    settings.LogName = "Application";
});
#endregion

builder.Services.Configure<PickerOptions>(
    builder.Configuration.GetSection("Picker"));

builder.Services.AddSingleton(
    new PickerRepository(
        builder.Configuration.GetConnectionString("DbssDbProd")));

builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();