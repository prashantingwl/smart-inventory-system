using Microsoft.EntityFrameworkCore;
using SmartInventory.Core.Entities;
using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Infrastructure.Data;

public class SmartInventoryDbContext : DbContext
{
    private readonly ITenantProvider _tenantProvider;

    public SmartInventoryDbContext(
        DbContextOptions<SmartInventoryDbContext> options,
        ITenantProvider tenantProvider)
        : base(options)
    {
        _tenantProvider = tenantProvider;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Product> Products => Set<Product>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ─── Tenant ───
        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.Name).IsRequired().HasMaxLength(200);
            entity.Property(t => t.Slug).IsRequired().HasMaxLength(100);
            entity.HasIndex(t => t.Slug).IsUnique();
        });

        // ─── Product ───
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            // SKU is unique per tenant (not globally)
            entity.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();

            entity.HasOne(p => p.Tenant)
                  .WithMany()
                  .HasForeignKey(p => p.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            // 🎯 Global query filter: only return rows for the current tenant
            entity.HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId());
        });
    }
}