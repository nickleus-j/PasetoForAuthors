using OpineHere.Data;
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

    public async Task InvokeAsync(HttpContext context, IAuthService authService,IDataUnitOfWork unitOfWork)
    {
        var token = authService.GetToken();
        
        if (!string.IsNullOrEmpty(token))
        {
            context.Items["IsAuthenticated"] = true;
            
            try
            {
                // Extract user information from token claims
                var userId = authService.GetUserId();
                var userDisplayName = await ExtractUserDisplayName(userId, unitOfWork);
                
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

    private async Task<string> ExtractUserDisplayName(string userId,IDataUnitOfWork unitOfWork)
    {
        
        try
        {
            var user=await unitOfWork.AuthorProfileRepo.GetProfileAsync(userId);
            return user.GivenName + " " + user.Surname;
        }
        catch
        {
            return "User"; // Fallback display name
        }
    }
}
