
using System.Text.Json;
using ApiGateway.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Transforms;



var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5000");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

builder.Services.AddHttpClient<DeviceClient>();
builder.Services.AddHttpClient<TelemetryClient>();
builder.Services.AddHttpClient<AuthClient>();

var redisConnection = builder.Configuration["Redis:Connection"];

if (string.IsNullOrEmpty(redisConnection))
    throw new Exception("Redis connection string missing");

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

// FIX: Add header forwarding for SignalR
var routes = new[]
{
    new RouteConfig
    {
        RouteId = "signalr_route",
        ClusterId = "signalr_cluster",
        Match = new RouteMatch
        {
            Path = "/telemetryHub/{**catch-all}",
            Methods = new List<string> { "GET", "POST", "OPTIONS" }
        }
    }
};

var clusters = new[]
{
    new ClusterConfig
    {
        ClusterId = "signalr_cluster",
        Destinations = new Dictionary<string, DestinationConfig>
        {
            {
                "dest1",
                new DestinationConfig
                {
                    Address = "http://telemetry-service:5001/"
                }
            }
        }
    }
};

// FIX: Add transforms to forward Authorization headers

builder.Services.AddReverseProxy()
    .LoadFromMemory(routes, clusters)
    .AddTransforms(builderContext =>
    {
        // Forward all headers including Authorization
        builderContext.AddRequestTransform(async context =>
        {
            // Don't modify anything - just log for debugging
            var token = context.HttpContext.Request.Query["access_token"].FirstOrDefault();
            if (!string.IsNullOrEmpty(token))
            {
                Console.WriteLine($"[YARP] Token from query (first 20 chars): {token.Substring(0, Math.Min(20, token.Length))}");
            }
            
            await ValueTask.CompletedTask;
        });
    });

var key = "super_secret_key_12345_super_secret_key_12345";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            ValidIssuer = "telemetry-app",
            ValidAudience = "telemetry-app",

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(key)
            ),

            ClockSkew = TimeSpan.FromMinutes(5)  // FIX: Change from Zero to 5 minutes
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var path = context.HttpContext.Request.Path;

                var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
                if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
                {
                    context.Token = authHeader.Substring("Bearer ".Length);
                    return Task.CompletedTask;
                }

                var accessToken = context.Request.Query["access_token"];
                if (!string.IsNullOrEmpty(accessToken) &&
                    path.StartsWithSegments("/telemetryHub"))
                {
                    context.Token = accessToken;
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseRouting();

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapReverseProxy();

app.Run();