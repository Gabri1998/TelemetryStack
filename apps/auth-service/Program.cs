using AuthService.Repositories;
using AuthService.Services;


var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseUrls("http://localhost:5003");

builder.Services.AddControllers();

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