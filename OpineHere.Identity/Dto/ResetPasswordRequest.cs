using System.ComponentModel.DataAnnotations;

namespace OpineHere.Identity.Dto;

public class ResetPasswordRequest
{
    [Required]
    public string UserId { get; set; }

    [Required]
    public string Token { get; set; }

    [Required, StringLength(100, MinimumLength = 8)]
    public string NewPassword { get; set; }

    [Required, Compare(nameof(NewPassword))]
    public string ConfirmPassword { get; set; }
}