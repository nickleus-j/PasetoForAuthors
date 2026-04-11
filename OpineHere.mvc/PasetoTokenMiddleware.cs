using OpineHere.mvc.Service;

namespace OpineHere.mvc;

public class PasetoTokenMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<PasetoTokenMiddleware> _logger;

    public PasetoTokenMiddleware(RequestDelegate next, ILogger<PasetoTokenMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuthService authService)
    {
        var token = authService.GetToken();
        
        if (!string.IsNullOrEmpty(token))
        {
            context.Items["IsAuthenticated"] = true;
            
            try
            {
                // Extract user information from token claims
                var userDisplayName = ExtractUserDisplayName(token);
                var userId = authService.GetUserId();
                
                context.Items["PasetoToken"] = token;
                context.Items["UserDisplayName"] = userDisplayName;
                context.Items["UserId"] = userId;
                
                _logger.LogInformation($"User authenticated: {userDisplayName}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting user info from token: {ex.Message}");
                context.Items["IsAuthenticated"] = false;
            }
        }
        else
        {
            context.Items["IsAuthenticated"] = false;
        }

        await _next(context);
    }

    private string ExtractUserDisplayName(string token)
    {
        // If using Paseto v4, you'll need to parse the token
        // This is a simplified example - adjust based on your token structure
        try
        {
            // Note: Paseto tokens are encrypted, so you need proper decryption keys
            // This example assumes you store the user email/name in claims during creation
            // You may need to call the API to validate and decode the token
            return "User"; // Fallback display name
        }
        catch
        {
            return "User";
        }
    }
}
