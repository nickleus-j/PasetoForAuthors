using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OpineHere.Data;
using OpineHere.EntityFramework;
using OpineHere.mvc;
using OpineHere.mvc.Service;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddUserSecrets<Program>(optional: true) // enables dotnet user-secrets override
    .AddEnvironmentVariables(); // enables Docker/env var override
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddTransient<IDataUnitOfWork, EfUnitOfWork>();
builder.Services.AddDbContext<OpineContext>(options =>
    options.UseMySQL(connectionString, b => b.MigrationsAssembly("OpineHere.mvc")));

//Identity Paseto
builder.Services.AddHttpClient<PasetoApiClient>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5100");
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});
builder.Services.AddAuthentication(options =>
    {
        // If your library provides a specific Paseto scheme name, use it here.
        // If you are storing the token in a standard cookie, you'd use CookieAuthenticationDefaults.AuthenticationScheme
        options.DefaultAuthenticateScheme = "Paseto"; 
        options.DefaultChallengeScheme = "Paseto";
    })
    .AddScheme<AuthenticationSchemeOptions, PasetoAuthenticationHandler>("Paseto", options => 
    {
        // If you have a custom Paseto Handler, it gets registered here.
        // Some libraries might offer a cleaner extension method like .AddPaseto(options => { ... })
    });
builder.Services.AddHttpContextAccessor();

// Add session management for token storage
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
});
builder.Services.AddPasetoAuthentication();
builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseSession();
app.UseRouting();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OpineContext>();
    db.Database.Migrate(); // Creates DB if missing, applies migrations, seeds data
}
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();
app.UseMiddleware<PasetoTokenMiddleware>();

app.Run();