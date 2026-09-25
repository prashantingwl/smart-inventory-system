using Microsoft.EntityFrameworkCore;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Services;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class CategoryService : ICategoryService
{
    private readonly SmartInventoryDbContext _db;

    public CategoryService(SmartInventoryDbContext db)
    {
        _db = db;
    }

    public async Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default)
    {
        category.Id = Guid.NewGuid();
        category.CreatedAtUtc = DateTime.UtcNow;

        _db.Categories.Add(category);
        await _db.SaveChangesAsync(cancellationToken);

        return category;
    }

    public async Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }
}