
using ApiGateway.Services;


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddHttpClient<DeviceClient>();
builder.Services.AddHttpClient<TelemetryClient>();

var redisConnection = builder.Configuration["Redis:Connection"];

if (string.IsNullOrEmpty(redisConnection))
    throw new Exception("Redis connection string missing");


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
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

builder.Services.AddReverseProxy()
    .LoadFromMemory(
        new[]
        {
            new Yarp.ReverseProxy.Configuration.RouteConfig
            {
                RouteId = "signalr_route",
                ClusterId = "signalr_cluster",
                Match = new Yarp.ReverseProxy.Configuration.RouteMatch
                {
                    Path = "/telemetryHub/{**catch-all}"
                }
            }
        },
        new[]
        {
            new Yarp.ReverseProxy.Configuration.ClusterConfig
            {
                ClusterId = "signalr_cluster",
                Destinations = new Dictionary<string, Yarp.ReverseProxy.Configuration.DestinationConfig>
                {
                    { "dest1", new Yarp.ReverseProxy.Configuration.DestinationConfig
                        {
                            Address = "http://localhost:5001/"
                        }
                    }
                }
            }
        });

var app = builder.Build();

// Enable Swagger only in development

    app.UseSwagger();
    app.UseSwaggerUI();


// Redirect HTTP → HTTPS (can ignore for now on Linux)
//app.UseHttpsRedirection();


// Enables routing system
app.UseRouting();

app.UseCors("AllowFrontend");
// Maps controller endpoints
app.MapControllers();
app.MapReverseProxy();

// Starts the server
app.Run();