using System.ComponentModel.DataAnnotations;

namespace sample_auth_aspnet.Models.Entities;

public class BaseEntity
{
    [Key]
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
