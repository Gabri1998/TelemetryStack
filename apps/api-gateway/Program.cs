
using ApiGateway.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<DeviceClient>();
builder.Services.AddHttpClient<TelemetryClient>();

var redisConnection = builder.Configuration["Redis:Connection"];

if (string.IsNullOrEmpty(redisConnection))
    throw new Exception("Redis connection string missing");

builder.Services.AddSingleton<IConnectionMultiplexer>(
    ConnectionMultiplexer.Connect(redisConnection)
);

// Swagger services (API documentation)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy
                .WithOrigins("http://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

var app = builder.Build();

// Enable Swagger only in development

    app.UseSwagger();
    app.UseSwaggerUI();


// Redirect HTTP → HTTPS (can ignore for now on Linux)
app.UseHttpsRedirection();

// Enables routing system
app.UseRouting();

app.UseCors("AllowFrontend");
// Maps controller endpoints
app.MapControllers();

// Starts the server
app.Run();