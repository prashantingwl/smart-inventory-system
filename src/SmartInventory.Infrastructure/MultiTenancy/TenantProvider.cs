using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Infrastructure.MultiTenancy;

/// <summary>
/// Reads the current tenant from the scoped TenantContext.
/// The TenantContext is populated by the API layer (see TenantResolutionMiddleware).
/// This class has NO knowledge of HTTP — it just reads a scoped value.
/// </summary>
public class TenantProvider : ITenantProvider
{
    private readonly TenantContext _context;

    public TenantProvider(TenantContext context)
    {
        _context = context;
    }

    public bool HasTenant => _context.TenantId.HasValue;

    public Guid GetTenantId()
    {
        if (_context.TenantId is null)
        {
            throw new InvalidOperationException(
                "No tenant has been resolved for the current request. " +
                "Ensure the 'X-Tenant-Id' header is present and valid.");
        }

        return _context.TenantId.Value;
    }
}