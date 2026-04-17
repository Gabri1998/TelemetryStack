using StackExchange.Redis;
using TelemetryService.Repositories;
using TelemetryService.Services;
using TelemetryService.Hubs;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://0.0.0.0:5001");

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

builder.Services.AddScoped<TelemetryRepository>();
builder.Services.AddSingleton<TelemetryProcessor>();
builder.Services.AddScoped<TelemetryQueryService>();
builder.Services.AddScoped<DeviceStatusService>();

builder.Services.AddHostedService<MqttWorker>();
builder.Services.AddHostedService<TelemetryDbWorker>();

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

            ClockSkew = TimeSpan.FromMinutes(5)  // FIX: Add clock skew
        };

        // FIX: Add better debugging
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "")
                    ?? context.Request.Query["access_token"].FirstOrDefault();

                Console.WriteLine($"[AUTH] Token received: {(token != null ? "YES" : "NO")}");
                Console.WriteLine($"[AUTH] Token length: {token?.Length ?? 0}");
                
                context.Token = token;
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"[AUTH] FAILED: {context.Exception.Message}");
                Console.WriteLine($"[AUTH] Exception type: {context.Exception.GetType().Name}");
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Console.WriteLine("[AUTH] Token validated successfully!");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"[AUTH] Challenge - Error: {context.Error}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseRouting();

app.UseCors("AllowFrontend");
app.UseSwagger();
app.UseSwaggerUI();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHub<TelemetryHub>("/telemetryHub")
   .RequireCors("AllowFrontend");

app.Run();