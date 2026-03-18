using ApiGateway.Repositories;
using ApiGateway.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Register controllers (enables API endpoints)
builder.Services.AddControllers();

// Register DeviceService in Dependency Injection container
// AddScoped means:
// - One instance per HTTP request
builder.Services.AddScoped<DeviceService>();


var redisConnection = builder.Configuration["Redis:Connection"];

if (string.IsNullOrEmpty(redisConnection))
    throw new Exception("Redis connection string missing");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection)
);

// Swagger services (API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<DeviceRepository>();

var app = builder.Build();

// Enable Swagger only in development
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Redirect HTTP → HTTPS (can ignore for now on Linux)
app.UseHttpsRedirection();

// Enables routing system
app.UseRouting();

// Maps controller endpoints
app.MapControllers();

// Starts the server
app.Run();