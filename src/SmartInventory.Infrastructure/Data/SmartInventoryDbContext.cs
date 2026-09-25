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
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
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

        // ─── Category ───
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).HasMaxLength(1000);

            // Category name is unique per tenant
            entity.HasIndex(c => new { c.TenantId, c.Name }).IsUnique();

            entity.HasOne(c => c.Tenant)
                  .WithMany()
                  .HasForeignKey(c => c.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(c => c.TenantId == _tenantProvider.GetTenantId());
        });

        // ─── Warehouse ───
        modelBuilder.Entity<Warehouse>(entity =>
        {
            entity.HasKey(w => w.Id);
            entity.Property(w => w.Name).IsRequired().HasMaxLength(200);
            entity.Property(w => w.Code).IsRequired().HasMaxLength(50);
            entity.Property(w => w.AddressLine).HasMaxLength(500);
            entity.Property(w => w.City).HasMaxLength(100);
            entity.Property(w => w.PinCode).HasMaxLength(20);

            // Warehouse code is unique per tenant
            entity.HasIndex(w => new { w.TenantId, w.Code }).IsUnique();

            entity.HasOne(w => w.Tenant)
                  .WithMany()
                  .HasForeignKey(w => w.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(w => w.TenantId == _tenantProvider.GetTenantId());
        });

        // ─── Product ───
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(200);
            entity.Property(p => p.Sku).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Description).HasMaxLength(2000);
            entity.Property(p => p.Price).HasPrecision(18, 2);

            entity.HasIndex(p => new { p.TenantId, p.Sku }).IsUnique();

            entity.HasOne(p => p.Tenant)
                  .WithMany()
                  .HasForeignKey(p => p.TenantId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasQueryFilter(p => p.TenantId == _tenantProvider.GetTenantId());
        });
    }
}