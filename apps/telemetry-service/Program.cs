using StackExchange.Redis;
using TelemetryService.Repositories;
using TelemetryService.Services;
using TelemetryService.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:5001");

// HTTP
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });


// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")
);

builder.Services.AddSignalR();

// Services
builder.Services.AddScoped<TelemetryRepository>();
builder.Services.AddSingleton<TelemetryProcessor>();
builder.Services.AddScoped<TelemetryQueryService>();
builder.Services.AddScoped<DeviceStatusService>();

// Workers
builder.Services.AddHostedService<MqttWorker>();
builder.Services.AddHostedService<TelemetryDbWorker>();


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
          
    });
});
var app = builder.Build();


app.UseRouting();

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.MapHub<TelemetryHub>("/telemetryHub")
   .RequireCors("AllowFrontend");

app.Run();