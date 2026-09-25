using SmartInventory.Core.MultiTenancy;

namespace SmartInventory.Api.Middleware;

/// <summary>
/// Reads the X-Tenant-Id header from the incoming request and stores it
/// in the scoped TenantContext so downstream services can access it.
/// </summary>
public class TenantResolutionMiddleware
{
    public const string HeaderName = "X-Tenant-Id";

    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        // Skip tenant resolution for Swagger and static assets
        var path = context.Request.Path.Value ?? "";
        if (path.StartsWith("/swagger", StringComparison.OrdinalIgnoreCase))
        {
            await _next(context);
            return;
        }

        var header = context.Request.Headers[HeaderName].FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(header) && Guid.TryParse(header, out var tenantId))
        {
            tenantContext.TenantId = tenantId;
        }

        await _next(context);
    }
}