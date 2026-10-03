namespace OpineHere.Identity.Service;

public interface IEmailSender
{
    Task SendEmailAsync(string email, string subject, string htmlMessage);
    Task SendMagicLinkAsync(string recipientEmail, string magicLink);
}