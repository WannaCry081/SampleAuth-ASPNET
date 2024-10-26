using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sample_auth_aspnet.Models.Entities;

[Index(nameof(JTI), IsUnique = true)]
public class Token : BaseEntity
{
    [ForeignKey("User")]
    public int UserId { get; init; }
    public string JTI { get; set; } = null!;
    public bool IsRevoked { get; set; } = false;

    // Navigation Properties
    public User User { get; set; } = null!;
}