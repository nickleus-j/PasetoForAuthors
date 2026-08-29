using System.ComponentModel.DataAnnotations;

namespace OpineHere.Identity.Dto;

public class ForgotPasswordRequest
{
    [Required, EmailAddress]
    public string Email { get; set; }
    public string ResetUrl { get; set; }
}