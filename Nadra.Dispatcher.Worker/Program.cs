using Nadra.Dispatcher.Worker;
using Nadra.Dispatcher.Worker.Options;
using Nadra.Dispatcher.Worker.Repositories;
using Nadra.Dispatcher.Worker.Services;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.Configure<DispatcherOptions>(
    builder.Configuration.GetSection("Dispatcher"));

builder.Services.AddSingleton(
    new TrackerRepository(
        builder.Configuration.GetConnectionString("DbssDbProd")));

builder.Services.AddSingleton(
    new AuditRepository(
        builder.Configuration.GetConnectionString("DbssDbProd")));

builder.Services.Configure<DispatcherOptions>(
    builder.Configuration.GetSection("Picker"));

builder.Services.AddHttpClient<NadraApiClient>(client =>
{
    client.BaseAddress = new Uri(
        builder.Configuration["NadraApi:BaseUrl"]!);

    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(
            builder.Configuration["NadraApi:TimeoutSeconds"]!));
});

builder.Services.AddSingleton<DispatchCoordinator>();
builder.Services.AddHostedService<Worker>();

await builder.Build().RunAsync();