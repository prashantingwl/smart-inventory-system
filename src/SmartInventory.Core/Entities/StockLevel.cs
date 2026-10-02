using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Core.Entities;

/// <summary>
/// Aggregate stock for a Product × Warehouse combination.
/// Derived from batches but stored separately for fast queries.
/// Recomputed on every stock movement.
/// </summary>
public class StockLevel : ITenantEntity
{
    public Guid Id { get; set; }
    public Guid TenantId { get; set; }
    public Guid ProductId { get; set; }
    public Guid WarehouseId { get; set; }

    /// <summary>Total physical quantity across all batches in this warehouse.</summary>
    public int QuantityOnHand { get; set; }

    /// <summary>Quantity allocated to pending orders but not yet shipped.</summary>
    public int QuantityReserved { get; set; }

    /// <summary>Available = OnHand - Reserved. What new orders can draw from.</summary>
    public int QuantityAvailable { get; set; }

    /// <summary>Earliest expiry across all batches. Drives FEFO warnings.</summary>
    public DateTime? EarliestExpiryDate { get; set; }

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public DateTime? LastMovementAtUtc { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public Product? Product { get; set; }
    public Warehouse? Warehouse { get; set; }
}