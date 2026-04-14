using System.Text.Json;
using AuthService.Repositories;
using AuthService.Services;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://0.0.0.0:5003");

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    });

// services
builder.Services.AddScoped<UserRepository>();
builder.Services.AddSingleton<JwtService>();
builder.Services.AddScoped<AuthServiceImpl>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();