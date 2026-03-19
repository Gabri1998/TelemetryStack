using Microsoft.Extensions.Hosting;
using StackExchange.Redis;
using TelemetryService.Repositories;
using TelemetryService.Services;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Redis
        services.AddSingleton<IConnectionMultiplexer>(
            ConnectionMultiplexer.Connect("localhost:6379")
        );

        // App services
        services.AddScoped<TelemetryRepository>();
        services.AddScoped<TelemetryProcessor>();

        // Workers
        services.AddHostedService<MqttWorker>();
        services.AddHostedService<TelemetryDbWorker>();
    });

var app = builder.Build();

await app.RunAsync();