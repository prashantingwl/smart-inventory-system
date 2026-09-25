using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartInventory.Api.Middleware;
using SmartInventory.Core.MultiTenancy;
using SmartInventory.Core.Services;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.MultiTenancy;
using SmartInventory.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// 🆕 Multi-tenancy
builder.Services.AddScoped<TenantContext>();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartInventory API",
        Version = "v1",
        Description = "Inventory management API"
    });
});

builder.Services.AddScoped<TenantInterceptor>();

builder.Services.AddDbContext<SmartInventoryDbContext>((sp, options) =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"));
    options.AddInterceptors(sp.GetRequiredService<TenantInterceptor>());
});

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IWarehouseService, WarehouseService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Swagger 3.0 → 3.1 middleware (keep this!)
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? "";
        if (path.Contains("swagger.json", StringComparison.OrdinalIgnoreCase))
        {
            var originalBody = context.Response.Body;
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await next();

            memStream.Position = 0;
            var body = await new StreamReader(memStream).ReadToEndAsync();

            var patched = System.Text.RegularExpressions.Regex.Replace(
                body,
                @"""openapi""\s*:\s*""3\.\d+\.\d+""",
                @"""openapi"": ""3.1.0""");

            context.Response.Headers.ContentLength = null;
            context.Response.Headers.Remove("Transfer-Encoding");

            var bytes = System.Text.Encoding.UTF8.GetBytes(patched);
            context.Response.ContentLength = bytes.Length;
            context.Response.Body = originalBody;

            await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
        else
        {
            await next();
        }
    });

    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SmartInventory API v1");
        options.RoutePrefix = "swagger";
    });
}

// 🆕 Tenant resolution middleware
app.UseMiddleware<TenantResolutionMiddleware>();

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();