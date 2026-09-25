using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Infrastructure.Data;

/// <summary>
/// Automatically stamps <c>TenantId</c> on every entity implementing
/// <see cref="ITenantEntity"/> before it is inserted. This guarantees
/// tenant isolation at the persistence layer, so individual services
/// cannot forget to set the tenant.
/// </summary>
public class TenantInterceptor : SaveChangesInterceptor
{
    private readonly ITenantProvider _tenantProvider;

    public TenantInterceptor(ITenantProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        StampTenant(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        StampTenant(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    private void StampTenant(DbContext? context)
    {
        if (context is null) return;

        // Don't try to resolve tenant during migrations / design-time tools
        if (!_tenantProvider.HasTenant) return;

        var tenantId = _tenantProvider.GetTenantId();

        var newTenantEntities = context.ChangeTracker
            .Entries<ITenantEntity>()
            .Where(e => e.State == EntityState.Added);

        foreach (var entry in newTenantEntities)
        {
            // Only set if not already set (allow explicit override)
            if (entry.Entity.TenantId == Guid.Empty)
            {
                entry.Entity.TenantId = tenantId;
            }
        }
    }
}