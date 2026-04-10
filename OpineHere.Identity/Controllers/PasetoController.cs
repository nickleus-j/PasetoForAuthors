using Microsoft.AspNetCore.Mvc;
using OpineHere.Identity.Token;
using OpineHere.Identity.Authentication;
using OpineHere.Identity.Service;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace OpineHere.Identity.Controllers;

[ApiController]
[Route("api/Identity")]
public class PasetoController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<PasetoController> _logger;

    public PasetoController(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService, 
        ILogger<PasetoController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
    }
    [HttpGet]
    public IActionResult Index()
    {
        return Ok("Hello World!");
    }
    /// <summary>
    /// Register a new user
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(request.Email);
            if (existingUser != null)
            {
                return BadRequest(new { 
                    message = "User with this email already exists" 
                });
            }

            // Create new user
            var user = new IdentityUser
            {
                UserName = request.Email,
                Email = request.Email,
                EmailConfirmed = false
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning($"Registration failed for {request.Email}: {errors}");
                return BadRequest(new { 
                    message = "Registration failed",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation($"User registered successfully: {request.Email}");

            return Ok(new
            {
                message = "User registered successfully",
                userId = user.Id,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Registration error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during registration" });
        }
    }
    /// <summary>
    /// Authenticate user and issue PASETO token
    /// </summary>
    [HttpPost("authenticate")]
    public async Task<IActionResult> Authenticate(
        [FromBody] AuthenticateRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            // Find user by email
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                _logger.LogWarning($"Login failed: User not found for email {request.Email}");
                return Unauthorized(new { 
                    message = "Invalid email or password" 
                });
            }

            // Verify password
            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!isPasswordValid)
            {
                _logger.LogWarning($"Login failed: Invalid password for user {request.Email}");
                return Unauthorized(new { 
                    message = "Invalid email or password" 
                });
            }

            // Generate token
            var token = _tokenService.GenerateToken(user.Id, request.Email);

            _logger.LogInformation($"User logged in successfully: {request.Email}");

            return Ok(new
            {
                token,
                expiresIn = "1h",
                tokenType = "Bearer",
                userId = user.Id,
                email = user.Email
            });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Login error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during login" });
        }
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