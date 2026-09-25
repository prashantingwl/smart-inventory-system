using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Core.Entities;

/// <summary>
/// A dark store / cold storage facility. Each warehouse supports a specific
/// temperature zone. A product can only be stored in a warehouse whose zone
/// matches the product's category requirement.
/// </summary>
public class Warehouse : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }

    public string Name { get; set; } = string.Empty;     // "Blinkit-Koramangala-01"
    public string Code { get; set; } = string.Empty;     // "BLR-KOR-01" (unique per tenant)

    public string? AddressLine { get; set; }
    public string? City { get; set; }
    public string? PinCode { get; set; }

    /// <summary>
    /// The temperature zone this warehouse is equipped to store.
    /// </summary>
    public TemperatureZone SupportedZone { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
}