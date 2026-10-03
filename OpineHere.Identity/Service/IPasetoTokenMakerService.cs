namespace OpineHere.Identity.Service;

public interface IPasetoTokenMakerService
{
    string GenerateMagicLinkToken(string authorId, string email);
    PasetoTokenValidationResult ValidateMagicLinkToken(string token);
}
public record PasetoTokenValidationResult(
    bool IsValid, 
    string? AuthorId, 
    string? Email, 
    string? TokenId, 
    string? ErrorMessage
);