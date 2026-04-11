namespace OpineHere.Identity.Authentication;

public class AuthResponse
{
    public string token{get;set;}
    public string expiresIn{get;set;}
    public string tokenType{get;set;}
    public string userId {get;set;}
    public string email {get;set;}
}