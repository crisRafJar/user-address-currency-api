using Microsoft.EntityFrameworkCore;
using UserAddressCurrencyApi.Domain.Entities;

namespace UserAddressCurrencyApi.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<Currency> Currencies => Set<Currency>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.Property(u => u.Name).IsRequired().HasMaxLength(200);
            e.Property(u => u.Email).IsRequired().HasMaxLength(320);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.PasswordHash).HasMaxLength(100);

            e.HasMany(u => u.Addresses)
                .WithOne(a => a.User)
                .HasForeignKey(a => a.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Address>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Street).IsRequired().HasMaxLength(300);
            e.Property(a => a.City).IsRequired().HasMaxLength(150);
            e.Property(a => a.Country).IsRequired().HasMaxLength(150);
            e.Property(a => a.ZipCode).HasMaxLength(20);
        });

        modelBuilder.Entity<Currency>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Code).IsRequired().HasMaxLength(10);
            e.HasIndex(c => c.Code).IsUnique();
            e.Property(c => c.Name).IsRequired().HasMaxLength(100);
            e.Property(c => c.RateToBase).IsRequired().HasPrecision(28, 10);
        });
    }
}
