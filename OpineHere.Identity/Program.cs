using OpineHere.Identity.Service;
using OpineHere.Identity.Token;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
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
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "JWT Authorization header using the Bearer scheme."
    });

    options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
    {
        [new OpenApiSecuritySchemeReference("bearer", document)] = []
    });
});

// Add services
builder.Services.AddSingleton<IKeyProvider, PasetoKeyProvider>();
builder.Services.AddScoped<ITokenService, PasetoTokenService>();
builder.Services.AddTransient<IDataUnitOfWork, EfUnitOfWork>();
builder.Services.AddDbContext<OpineContext>(options =>
    options.UseMySQL(connectionString, b => b.MigrationsAssembly("OpineHere.Identity")));
// Add Identity
builder.Services.AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        // Password requirements
        options.Password.RequireDigit = true;
        options.Password.RequiredLength = 8;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireLowercase = true;
    
        // Lockout settings
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;
    
        // User settings
        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<OpineContext>()
    .AddDefaultTokenProviders();
builder.Services.AddAuthentication()
    .AddScheme<AuthenticationSchemeOptions, PasetoBearerHandler>(
        "PasetoBearerScheme", 
        options => { });
builder.Services.AddLogging();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger(options =>{});
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API");
        options.RoutePrefix = string.Empty; // Swagger at root URL
    });
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();