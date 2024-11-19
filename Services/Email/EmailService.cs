using System.Net;
using System.Net.Mail;
using sample_auth_aspnet.Models.Utils;

namespace sample_auth_aspnet.Services.Email;

public class EmailService(
    ILogger<EmailService> logger,
    SMTPSettings smtp) : IEmailService
{
    public async Task SendResetEmailAsync(string toEmail, string link)
    {
        var htmlTemplate = EmailTemplate.GetForgotPasswordTemplate()
            .Replace("{{email}}", toEmail)
            .Replace("{{reset_link}}", link);

        var mailMessage = new MailMessage()
        {
            From = new MailAddress(smtp.Username, "Sample Authentication"),
            Subject = "Password Reset Request",
            Body = htmlTemplate,
            IsBodyHtml = true
        };

        mailMessage.To.Add(toEmail);

        using var smtpClient = new SmtpClient(smtp.Server, smtp.Port)
        {
            Credentials = new NetworkCredential(smtp.Username, smtp.Password),
            EnableSsl = true
        };

        logger.LogInformation("Sending email to {Email}", toEmail);
        await smtpClient.SendMailAsync(mailMessage);
    }
}
