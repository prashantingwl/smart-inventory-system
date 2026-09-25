namespace SmartInventory.Core.MultiTenancy;

/// <summary>
/// Marks an entity as tenant-scoped. The TenantInterceptor will automatically
/// set <see cref="TenantId"/> when the entity is inserted.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; set; }
}