using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Contracts;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Services;

namespace SmartInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WarehousesController : ControllerBase
{
    private readonly IWarehouseService _service;

    public WarehousesController(IWarehouseService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<WarehouseResponse>> Create(
        [FromBody] CreateWarehouseRequest request,
        CancellationToken cancellationToken)
    {
        var warehouse = new Warehouse
        {
            Name = request.Name,
            Code = request.Code,
            AddressLine = request.AddressLine,
            City = request.City,
            PinCode = request.PinCode,
            SupportedZone = request.SupportedZone
        };

        var created = await _service.CreateAsync(warehouse, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<WarehouseResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var warehouse = await _service.GetByIdAsync(id, cancellationToken);
        if (warehouse is null) return NotFound();
        return Ok(Map(warehouse));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<WarehouseResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var warehouses = await _service.GetAllAsync(cancellationToken);
        return Ok(warehouses.Select(Map).ToList());
    }

    private static WarehouseResponse Map(Warehouse w) =>
        new(w.Id, w.Name, w.Code, w.AddressLine, w.City, w.PinCode,
            w.SupportedZone, w.IsActive, w.CreatedAtUtc);
}