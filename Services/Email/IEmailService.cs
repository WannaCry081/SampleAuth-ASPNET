namespace sample_auth_aspnet.Services.Email;

public interface IEmailService
{
    Task<bool> SendResetEmailAsync(string toEmail, string link);
}
