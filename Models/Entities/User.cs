namespace sample_auth_aspnet.Models.Entities;

[Index(nameof(Email), IsUnique = true)]
public sealed class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string Password { get; set; } = null!;

    // Navigation Properties
    public ICollection<Token> Tokens { get; set; } = [];
}
