using Microsoft.EntityFrameworkCore;
using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.Data;

public partial class DataContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasIndex(u => new
            {
                u.Email,
            })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}

