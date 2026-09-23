using Microsoft.EntityFrameworkCore;
using SmartInventory.Core.Entities;

namespace SmartInventory.Infrastructure.Data;

public class SmartInventoryDbContext : DbContext
{
    public SmartInventoryDbContext(DbContextOptions<SmartInventoryDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(50);

            entity.HasIndex(p => p.Sku)
                .IsUnique();

            entity.Property(p => p.Description)
                .HasMaxLength(2000);

            entity.Property(p => p.Price)
                .HasPrecision(18, 2);

            entity.Property(p => p.CreatedAtUtc)
                .IsRequired();
        });
    }
}