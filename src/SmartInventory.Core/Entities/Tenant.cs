namespace SmartInventory.Core.Entities;

public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;   // e.g., "blinkit", "zepto"
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; }
}