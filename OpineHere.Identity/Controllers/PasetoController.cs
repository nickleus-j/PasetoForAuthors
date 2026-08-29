using Microsoft.AspNetCore.Mvc;
using OpineHere.Identity.Token;
using OpineHere.Identity.Authentication;
using OpineHere.Identity.Service;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using OpineHere.Data;
using OpineHere.Identity.Dto;

namespace OpineHere.Identity.Controllers;

[ApiController]
[Route("api/Identity")]
public class PasetoController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<PasetoController> _logger;
    private IDataUnitOfWork _unitOfWork;
    private IEmailSender _emailSender;

    public PasetoController(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService, 
        IDataUnitOfWork unitOfWork,
        IEmailSender emailSender,
        ILogger<PasetoController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
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
            
            await _unitOfWork.AuthorProfileRepo.RegisterNewUserAsAuthorAsync(user.Id, request.GivenName,
                request.Surname);
            _logger.LogInformation($"User registered successfully: {request.Email}");

            var token = _tokenService.GenerateToken(user.Id, request.Email);

            _logger.LogInformation($"User logged in successfully: {request.Email}");

            return Ok(new AuthResponse
            {
                token=token,
                expiresIn = "1h",
                tokenType = "Bearer",
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

            return Ok(new AuthResponse
            {
                token=token,
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
    /// <summary>
    /// Request a password reset link via email
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.IsEmailConfirmedAsync(user))
            {
                // Always return OK to prevent email enumeration attacks
                return Ok(new { message = "If that email exists, a reset link has been sent." });
            }

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // URL-encode the token since it may contain special characters
            var encodedToken = WebEncoders.Base64UrlEncode(
                Encoding.UTF8.GetBytes(resetToken));

            // Build the callback URL pointing to your MVC client
            var callbackUrl = $"{request.ResetUrl}?userId={user.Id}&token={encodedToken}";

            await _emailSender.SendEmailAsync(
                user.Email,
                "Reset your password",
                $"Reset your password by clicking <a href=\"{callbackUrl}\">here</a>.");

            _logger.LogInformation($"Password reset link sent to {request.Email}");

            return Ok(new { message = "If that email exists, a reset link has been sent." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Forgot password error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred while processing your request" });
        }
    }

    /// <summary>
    /// Reset password using the token from the email link
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                return BadRequest(new { message = "Invalid reset request" });

            // Decode the Base64Url-encoded token
            var decodedToken = Encoding.UTF8.GetString(
                WebEncoders.Base64UrlDecode(request.Token));

            var result = await _userManager.ResetPasswordAsync(
                user, decodedToken, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                _logger.LogWarning($"Password reset failed for {request.UserId}: {errors}");
                return BadRequest(new
                {
                    message = "Password reset failed",
                    errors = result.Errors.Select(e => e.Description)
                });
            }

            _logger.LogInformation($"Password reset successfully for user {request.UserId}");

            return Ok(new { message = "Password has been reset successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Reset password error: {ex.Message}");
            return StatusCode(500, new { message = "An error occurred during password reset" });
        }
    }
}