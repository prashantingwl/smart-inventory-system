using SmartInventory.Core.Entities;

namespace SmartInventory.Core.Services;

public interface IWarehouseService
{
    Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken cancellationToken = default);
    Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default);
}