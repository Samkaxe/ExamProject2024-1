using Auth.Domain.BusinessEntities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Database;

public class DatabaseContext : DbContext
{
    public DbSet<Credentials> Credentials { get; set; }

    public DatabaseContext(DbContextOptions options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Set the primary key
        modelBuilder.Entity<Credentials>()
            .HasKey(c => c.UserId);

        // Ensure email is unique
        modelBuilder.Entity<Credentials>()
            .HasIndex(c => c.Email)
            .IsUnique();

        // Configure columns for Credentials
        modelBuilder.Entity<Credentials>()
            .Property(c => c.UserId)
            .ValueGeneratedOnAdd(); // Ensures the GUID is automatically generated

        modelBuilder.Entity<Credentials>()
            .Property(c => c.Email)
            .IsRequired()
            .HasMaxLength(256); // Typical max length for email addresses

        modelBuilder.Entity<Credentials>()
            .Property(c => c.PasswordHash)
            .IsRequired();

        modelBuilder.Entity<Credentials>()
            .Property(c => c.Salt)
            .IsRequired();
    }
}