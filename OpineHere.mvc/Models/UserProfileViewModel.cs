namespace OpineHere.mvc.Models;

public class UserProfileViewModel
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool HasToken { get; set; }
}