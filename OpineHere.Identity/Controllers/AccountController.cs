using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpineHere.Data;
using OpineHere.Identity.Service;
using OpineHere.Identity.Token;

namespace OpineHere.Identity.Controllers;

public class AccountController : Controller
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly ILogger<PasetoController> _logger;
    private IDataUnitOfWork _unitOfWork;
    private IEmailSender _emailSender;
    private IPasetoTokenMakerService _pasetoService;
    private readonly ITokenRevocationStore _revocationStore;

    public AccountController(
        UserManager<IdentityUser> userManager,
        ITokenService tokenService, 
        IDataUnitOfWork unitOfWork,
        IEmailSender emailSender,
        IPasetoTokenMakerService pasetoService,
        ITokenRevocationStore revocationStore,
        ILogger<PasetoController> logger)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _logger = logger;
        _unitOfWork = unitOfWork;
        _emailSender = emailSender;
    }
    
    [HttpPost("request-magic-link")]
    public async Task<IActionResult> RequestMagicLink([FromBody] string email,string baseUrl)
    {
        string token = _pasetoService.GenerateMagicLinkToken(email,email);
        string magicLink = $"{baseUrl}?token={Uri.EscapeDataString(token)}";

        await _emailSender.SendMagicLinkAsync(email, magicLink);

        return Ok(new { message = "If the email is registered, a magic login link has been sent." });
    }
    [HttpGet("verify-magic-link")]
    public async Task<IActionResult> VerifyMagicLink([FromQuery] string token)
    {
        var result = _pasetoService.ValidateMagicLinkToken(token);
        if (!result.IsValid)
        {
            return BadRequest(new { error = result.ErrorMessage });
        }

        if (await _revocationStore.IsTokenUsedAsync(result.TokenId!))
        {
            return BadRequest(new { error = "This magic link has already been used." });
        }

        // Mark token as used to prevent replay attacks
        await _revocationStore.MarkTokenAsUsedAsync(result.TokenId!, TimeSpan.FromMinutes(15));

        // Issue author session / cookie / access token
        return Ok(new { 
            message = "Authentication successful", 
            authorId = result.AuthorId, 
            email = result.Email 
        });
    }
}