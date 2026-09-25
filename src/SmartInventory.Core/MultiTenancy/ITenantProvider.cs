namespace SmartInventory.Core.MultiTenancy;

public interface ITenantProvider
{
    /// <summary>
    /// Returns the current tenant's Id. Throws if no tenant is resolved.
    /// </summary>
    Guid GetTenantId();

    /// <summary>
    /// Returns true if a tenant has been resolved for the current scope.
    /// </summary>
    bool HasTenant { get; }
}