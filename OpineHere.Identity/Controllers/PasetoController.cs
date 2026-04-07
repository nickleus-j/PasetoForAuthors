using Microsoft.AspNetCore.Mvc;
using OpineHere.Identity.Token;
using OpineHere.Identity.Authentication;
using OpineHere.Identity.Service;
namespace OpineHere.Identity.Controllers;

[ApiController]
[Route("api/Identity")]
public class PasetoController : Controller
{
    private readonly ITokenService _tokenService;
    private readonly ILogger<PasetoController> _logger;

    public PasetoController(
        ITokenService tokenService, 
        ILogger<PasetoController> logger)
    {
        _tokenService = tokenService;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello World!");
    }
    /// <summary>
    /// Authenticate user and issue PASETO token
    /// </summary>
    [HttpPost("authenticate")]
    public IActionResult Authenticate(
        [FromBody] AuthenticateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        
        if (string.IsNullOrEmpty(request.Email) || 
            string.IsNullOrEmpty(request.Password))
        {
            return Unauthorized(new { 
                message = "Invalid credentials" 
            });
        }

        var userId = Guid.NewGuid().ToString(); // Get from DB
        var token = _tokenService.GenerateToken(
            userId, 
            request.Email);

        return Ok(new
        {
            token,
            expiresIn = "1h",
            tokenType = "Bearer"
        });
    }

    /// <summary>
    /// Validate PASETO token (for service-to-service calls)
    /// </summary>
    [HttpPost("validate")]
    public IActionResult ValidateToken(
        [FromBody] TokenValidationRequest request)
    {
        var result = _tokenService.ValidateToken(request.Token);

        if (!result.IsValid)
            return Unauthorized(new { message = result.ErrorMessage });

        return Ok(new
        {
            isValid = true,
            userId = result.UserId,
            email = result.Email
        });
    }

    /// <summary>
    /// Protected endpoint for testing
    /// </summary>
    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        var userId = User.FindFirst("sub")?.Value;
        var email = User.FindFirst("email")?.Value;

        if (string.IsNullOrEmpty(userId))
            return Unauthorized();

        return Ok(new
        {
            userId,
            email
        });
    }

    /// <summary>
    /// Refresh token endpoint
    /// </summary>
    [HttpPost("refresh")]
    public IActionResult RefreshToken(
        [FromBody] RefreshTokenRequest request)
    {
        var validationResult = 
            _tokenService.ValidateToken(request.Token);

        if (!validationResult.IsValid)
            return Unauthorized(new { 
                message = "Invalid token" 
            });

        // Issue a new token
        var newToken = _tokenService.GenerateToken(
            validationResult.UserId,
            validationResult.Email,
            TimeSpan.FromHours(1));

        return Ok(new
        {
            token = newToken,
            expiresIn = "1h"
        });
    }
}