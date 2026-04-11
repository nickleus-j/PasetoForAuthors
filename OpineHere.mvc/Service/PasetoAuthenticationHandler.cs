using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace OpineHere.mvc.Service;

public static class PasetoAuthenticationExtensions
{
    public static void AddPasetoAuthentication(this IServiceCollection services)
    {
        services.AddAuthentication("PasetoScheme")
            .AddScheme<AuthenticationSchemeOptions, PasetoAuthenticationHandler>(
                "PasetoScheme", options => { });
    }
}

public class PasetoAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IAuthService _authService;
    private readonly ILogger<PasetoAuthenticationHandler> _logger;

    public PasetoAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IAuthService authService) : base(options, logger, encoder, clock)
    {
        _authService = authService;
        _logger = logger.CreateLogger<PasetoAuthenticationHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var token = _authService.GetToken();

        if (string.IsNullOrEmpty(token))
            return AuthenticateResult.NoResult();

        try
        {
            // In production, validate and parse the Paseto token here
            // For now, we'll create a basic principal
            var principal = new ClaimsPrincipal(
                new ClaimsIdentity(
                    new[]
                    {
                        //new Claim(ClaimTypes.NameIdentifier, _authService.GetUserId() ?? ""),
                        new Claim(ClaimTypes.Name, Request.HttpContext.Session.GetString("user_email") ?? ""),
                    },
                    "PasetoScheme"
                )
            );

            var ticket = new AuthenticationTicket(principal, "PasetoScheme");
            return AuthenticateResult.Success(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Authentication error: {ex.Message}");
            return AuthenticateResult.Fail("Invalid token");
        }
    }
}