namespace sample_auth_aspnet.Services.Email;

public interface IEmailService
{
    Task SendResetEmailAsync(string toEmail, string link);
}
