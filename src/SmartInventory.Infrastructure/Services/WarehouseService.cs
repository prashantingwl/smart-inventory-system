using Microsoft.EntityFrameworkCore;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Services;
using SmartInventory.Infrastructure.Data;

namespace SmartInventory.Infrastructure.Services;

public class WarehouseService : IWarehouseService
{
    private readonly SmartInventoryDbContext _db;

    public WarehouseService(SmartInventoryDbContext db)
    {
        _db = db;
    }

    public async Task<Warehouse> CreateAsync(Warehouse warehouse, CancellationToken cancellationToken = default)
    {
        warehouse.Id = Guid.NewGuid();
        warehouse.CreatedAtUtc = DateTime.UtcNow;

        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync(cancellationToken);

        return warehouse;
    }

    public async Task<Warehouse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _db.Warehouses
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Warehouse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _db.Warehouses
            .AsNoTracking()
            .OrderBy(w => w.Name)
            .ToListAsync(cancellationToken);
    }
}