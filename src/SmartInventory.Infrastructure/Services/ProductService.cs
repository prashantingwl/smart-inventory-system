using Microsoft.EntityFrameworkCore;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Services;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly SmartInventoryDbContext _db;

    public ProductService(SmartInventoryDbContext db)
    {
        _db = db;
    }

    public async Task<Product> CreateAsync(Product product, CancellationToken cancellationToken = default)
    {
        product.Id = Guid.NewGuid();
        product.CreatedAtUtc = DateTime.UtcNow;

        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        return product;
    }

    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Products
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
}