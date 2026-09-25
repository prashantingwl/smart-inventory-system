using SmartInventory.Core.Entities;

namespace SmartInventory.Core.Services;

public interface ICategoryService
{
    Task<Category> CreateAsync(Category category, CancellationToken cancellationToken = default);
    Task<Category?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Category>> GetAllAsync(CancellationToken cancellationToken = default);
}