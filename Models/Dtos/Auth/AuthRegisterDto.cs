namespace sample_auth_aspnet.Models.Dtos.Auth;

public record AuthRegisterDto(
    string Email,
    string FirstName,
    string LastName,
    string Password,
    string RePassword
);
