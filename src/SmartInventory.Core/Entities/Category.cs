using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Core.Entities;

public class Category : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    /// <summary>
    /// The temperature zone products in this category require.
    /// e.g., "Dairy" → Chilled, "Frozen Desserts" → Frozen.
    /// </summary>
    public TemperatureZone RequiredZone { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public ICollection<Product> Products { get; set; } = new List<Product>();
}