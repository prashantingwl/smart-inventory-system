using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using SmartInventory.Core.Services;
using SmartInventory.Infrastructure.Data;
using SmartInventory.Infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SmartInventory API",
        Version = "v1",
        Description = "Inventory management API"
    });
});

builder.Services.AddDbContext<SmartInventoryDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddScoped<IProductService, ProductService>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.Use(async (context, next) =>
    {
        var path = context.Request.Path.Value ?? "";

        if (path.Contains("swagger.json", StringComparison.OrdinalIgnoreCase))
        {
            // Capture the original body stream
            var originalBody = context.Response.Body;

            // Buffer the response
            using var memStream = new MemoryStream();
            context.Response.Body = memStream;

            await next();

            // Read what Swagger wrote
            memStream.Position = 0;
            var body = await new StreamReader(memStream).ReadToEndAsync();

            Console.WriteLine($"🔧 Original: {body.Substring(0, Math.Min(60, body.Length))}");

            // Patch the openapi version
            var patched = System.Text.RegularExpressions.Regex.Replace(
                body,
                @"""openapi""\s*:\s*""3\.\d+\.\d+""",
                @"""openapi"": ""3.1.0""");

            Console.WriteLine($"🔧 Patched: {patched != body}");

            // Reset EVERYTHING that might have been set
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

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();