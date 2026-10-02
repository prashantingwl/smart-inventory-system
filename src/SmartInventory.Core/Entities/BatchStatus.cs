namespace SmartInventory.Core.Entities;

/// <summary>
/// Lifecycle status of a batch. FEFO logic uses this to prioritize picking.
/// </summary>
public enum BatchStatus
{
    /// <summary>Normal, sellable, well within shelf life.</summary>
    Fresh,

    /// <summary>Approaching expiry — needs attention but still sellable.</summary>
    NearExpiry,

    /// <summary>Past the expiry date — must not be sold.</summary>
    Expired,

    /// <summary>Confirmed unusable (physical damage, contamination, etc.).</summary>
    Spoiled,

    /// <summary>Under investigation — pending QA decision (e.g., temperature excursion).</summary>
    Quarantined
}