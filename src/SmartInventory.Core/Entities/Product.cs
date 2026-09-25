using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Core.Entities;


public class Product : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }                 // NEW
    public string Name { get; set; } = string.Empty;
    public string Sku { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }                // NEW
}