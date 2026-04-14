using StackExchange.Redis;
using TelemetryService.Repositories;
using TelemetryService.Services;
using TelemetryService.Hubs;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5001");

// HTTP
builder.Services.AddControllers()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });


// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var config = sp.GetRequiredService<IConfiguration>();
    var redisConnection = config["Redis:Connection"];

    if (string.IsNullOrEmpty(redisConnection))
        throw new Exception("Redis connection string missing");

    var options = ConfigurationOptions.Parse(redisConnection);

    options.AbortOnConnectFail = false;
    options.ConnectRetry = 5;
    options.ConnectTimeout = 5000;
    options.SyncTimeout = 5000;

    while (true)
    {
        try
        {
            Console.WriteLine("Connecting to Redis...");
            var connection = ConnectionMultiplexer.Connect(options);
            Console.WriteLine("Connected to Redis");
            return connection;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Redis connection failed: {ex.Message}");
            Thread.Sleep(2000);
        }
    }
});

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