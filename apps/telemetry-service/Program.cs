using StackExchange.Redis;
using TelemetryService.Repositories;
using TelemetryService.Services;

var builder = WebApplication.CreateBuilder(args);

// HTTP
builder.Services.AddControllers();


// Redis
builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect("localhost:6379")
);

// Services
builder.Services.AddScoped<TelemetryRepository>();
builder.Services.AddScoped<TelemetryProcessor>();
builder.Services.AddScoped<TelemetryQueryService>();

// Workers
builder.Services.AddHostedService<MqttWorker>();
builder.Services.AddHostedService<TelemetryDbWorker>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();