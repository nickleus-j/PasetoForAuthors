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

        return View(new RegisterRequest());
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
}