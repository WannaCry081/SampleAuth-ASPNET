using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.Data;

public partial class DataContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>()
            .HasMany(u => u.Tokens)
            .WithOne(t => t.User)
            .HasForeignKey(t => t.UserId);

        modelBuilder.Entity<User>()
            .HasIndex(u => new
            {
                u.Email,
            })
            .IsUnique();

        modelBuilder.Entity<Token>()
            .HasIndex(t => new
            {
                t.Key
            })
            .IsUnique();

        base.OnModelCreating(modelBuilder);
    }
}
