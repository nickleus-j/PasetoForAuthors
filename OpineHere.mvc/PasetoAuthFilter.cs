using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using OpineHere.mvc.Service;

namespace OpineHere.mvc;


public class PasetoAuthFilter : IAuthorizationFilter
{
    private readonly IAuthService _authService;

    public PasetoAuthFilter(IAuthService authService)
    {
        _authService = authService;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        // Use your existing method
        if (!_authService.IsAuthenticated())
        {
            // If not authenticated, redirect to Login
            context.Result = new RedirectToActionResult("Login", "Account", new { returnUrl = context.HttpContext.Request.Path });
        }
    }
}
public class PasetoAuthorizeAttribute : TypeFilterAttribute
{
    public PasetoAuthorizeAttribute() : base(typeof(PasetoAuthFilter))
    {
    }
}