namespace SmartInventory.Core.MultiTenancy;

/// <summary>
/// Holds the current tenant for the lifetime of a request/scope.
/// Populated by the API layer via middleware.
/// </summary>
public class TenantContext
{
    public Guid? TenantId { get; set; }
}