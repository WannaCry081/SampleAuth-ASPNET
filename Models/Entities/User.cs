using Microsoft.EntityFrameworkCore;

namespace sample_auth_aspnet.Models.Entities;

[Index(nameof(Email), nameof(UserName), IsUnique = true)]
public sealed class User : BaseEntity
{
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Password { get; set; } = null!;
}
