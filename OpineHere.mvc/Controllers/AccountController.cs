using Microsoft.AspNetCore.Mvc;
using OpineHere.mvc.Models;
using OpineHere.mvc.Service;

namespace OpineHere.mvc.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        return View(new RegisterRequest(){GivenName = "",Email = "",Password = ""});
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        if (request.Password != request.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match");
            return View(request);
        }

        var success = await _authService.RegisterAsync(request);
        if (success)
        {
            _logger.LogInformation($"User {request.Email} registered successfully");
            return RedirectToAction("Index", "Home");
        }

        ModelState.AddModelError("", "Registration failed. Please try again.");
        return View(request);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        if (User.Identity?.IsAuthenticated == true)
            return RedirectToAction("Index", "Home");

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    // Controllers/AccountController.cs (Updated sections)
    [HttpPost]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (!ModelState.IsValid)
            return View(request);

        var success = await _authService.LoginAsync(request);
        if (success)
        {
            var userEmail = _authService.GetUserEmail();
            TempData["SuccessMessage"] = $"Welcome back, {userEmail}!";
            _logger.LogInformation($"User {userEmail} logged in successfully");
            return RedirectToAction("Index", "Home");
        }

        TempData["ErrorMessage"] = "Invalid email or password. Please try again.";
        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        var userEmail = _authService.GetUserEmail();
        await _authService.LogoutAsync();
        TempData["SuccessMessage"] = "You have been logged out successfully.";
        _logger.LogInformation($"User {userEmail} logged out");
        return RedirectToAction("Index", "Home");
    }
    [HttpGet("ForgotPassword")]
        public IActionResult ForgotPassword()
        {
            if (_authService.IsAuthenticated())
                return RedirectToAction("Index", "Home");

            return View();
        }

        [HttpPost("ForgotPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var resetUrl = $"{Request.Scheme}://{Request.Host}/ResetPassword";
            var (success, message) = await _authService.ForgotPasswordAsync(model.Email, resetUrl);

            TempData["Info"] = message ?? "If that email exists, a reset link has been sent.";

            if (success)
                return RedirectToAction("ForgotPasswordConfirmation");

            ModelState.AddModelError(string.Empty, message ?? "Failed to process request.");
            return View(model);
        }

        [HttpGet("ForgotPasswordConfirmation")]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet("ResetPassword")]
        public IActionResult ResetPassword(string? userId, string? token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                ModelState.AddModelError(string.Empty, "Invalid or expired reset link.");
                return View("ResetPasswordError");
            }

            return View(new ResetPasswordViewModel { UserId = userId, Token = token });
        }

        [HttpPost("ResetPassword")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var (success, message, errors) = await _authService.ResetPasswordAsync(
                model.UserId, model.Token, model.NewPassword, model.ConfirmPassword);

            if (success)
            {
                TempData["Success"] = message ?? "Password has been reset successfully.";
                return RedirectToAction("Login");
            }

            if (errors != null)
            {
                foreach (var error in errors)
                    ModelState.AddModelError(string.Empty, error);
            }
            else
            {
                ModelState.AddModelError(string.Empty, message ?? "Password reset failed.");
            }

            return View(model);
        }

        [HttpGet("ResetPasswordError")]
        public IActionResult ResetPasswordError()
        {
            return View();
        }
        [HttpGet("Me")]
        public IActionResult Me()
        {
            if (!_authService.IsAuthenticated())
                return RedirectToAction("Login");

            return View(new UserProfileViewModel
            {
                UserId = _authService.GetUserId(),
                Email = _authService.GetUserEmail(),
                HasToken = !string.IsNullOrEmpty(_authService.GetToken())
            });
        }
}