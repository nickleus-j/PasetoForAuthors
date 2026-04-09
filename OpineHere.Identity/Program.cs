using OpineHere.Identity.Service;
using OpineHere.Identity.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpineHere.Data;
using OpineHere.EntityFramework;
using OpineHere.Identity.Authentication;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true) // enables dotnet user-secrets override
    .AddEnvironmentVariables(); // enables Docker/env var override
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
// Add services
builder.Services.AddSingleton<IKeyProvider, PasetoKeyProvider>();
builder.Services.AddScoped<ITokenService, PasetoTokenService>();
builder.Services.AddTransient<IDataUnitOfWork, EfUnitOfWork>();
builder.Services.AddDbContext<OpineContext>(options =>
    options.UseMySQL(connectionString, b => b.MigrationsAssembly("opine")));

builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, PasetoBearerHandler>(
        "PasetoBearerScheme", 
        options => { });
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();
var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}