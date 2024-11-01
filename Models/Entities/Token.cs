using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace sample_auth_aspnet.Models.Entities;

[Index(nameof(Refresh), IsUnique = true)]
public class Token : BaseEntity
{
    [ForeignKey("User")]
    public int UserId { get; init; }
    public string Refresh { get; set; } = null!;
    public DateTime Expiration { get; init; }

    // Navigation Properties
    public User User { get; set; } = null!;
}