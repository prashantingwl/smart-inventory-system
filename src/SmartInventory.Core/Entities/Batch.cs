using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Core.Entities;

/// <summary>
/// A specific lot of a product with its own manufacturing and expiry dates.
/// Same product, different batches = same SKU, different expiry.
/// FEFO picking uses ExpiryDate to prioritize which batch to pick.
/// </summary>
public class Batch : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }

    /// <summary>Human-readable batch identifier (e.g., "MILK-20260926-A").</summary>
    public string BatchNumber { get; set; } = string.Empty;

    /// <summary>Current quantity in this batch (in the product's unit of measure).</summary>
    public int QuantityOnHand { get; set; }

    public DateTime ManufacturingDate { get; set; }

    /// <summary>The critical field for FEFO — batches with earlier dates get picked first.</summary>
    public DateTime ExpiryDate { get; set; }

    public BatchStatus Status { get; set; } = BatchStatus.Fresh;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}