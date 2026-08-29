namespace OpineHere.Identity.Service;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
}