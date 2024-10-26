using Microsoft.EntityFrameworkCore;
using sample_auth_aspnet.Models.Entities;

namespace sample_auth_aspnet.Data;

public partial class DataContext(DbContextOptions<DataContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Token> Tokens { get; set; }
}
