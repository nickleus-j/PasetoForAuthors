using Microsoft.Extensions.Options;
using OpineHere.Identity.Token;
using Microsoft.AspNetCore.Authentication;
using System.Text.Encodings.Web;

namespace OpineHere.Identity.Authentication;

public class PasetoBearerHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ITokenService _tokenService;

    public PasetoBearerHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ITokenService tokenService)
        : base(options, logger, encoder)
    {
        _tokenService = tokenService;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers.Authorization.FirstOrDefault();
        
        if (string.IsNullOrEmpty(authHeader) || 
            !authHeader.StartsWith("Bearer ", 
                StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }

        var token = authHeader["Bearer ".Length..].Trim();

        var validationResult = _tokenService.ValidateToken(token);
        
        if (!validationResult.IsValid)
        {
            return AuthenticateResult.Fail(
                validationResult.ErrorMessage);
        }

        var claims = new List<System.Security.Claims.Claim>
        {
            new("sub", validationResult.UserId),
            new("email", validationResult.Email)
        };

        var principal = new System.Security.Claims.ClaimsPrincipal(
            new System.Security.Claims.ClaimsIdentity(
                claims, 
                Scheme.Name));

        return AuthenticateResult.Success(
            new AuthenticationTicket(principal, Scheme.Name));
    }
}