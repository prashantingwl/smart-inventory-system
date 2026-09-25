using Microsoft.AspNetCore.Mvc;
using SmartInventory.Api.Contracts;
using SmartInventory.Core.Entities;
using SmartInventory.Core.Services;

namespace SmartInventory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _service;

    public CategoriesController(ICategoryService service)
    {
        _service = service;
    }

    [HttpPost]
    public async Task<ActionResult<CategoryResponse>> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            RequiredZone = request.RequiredZone
        };

        var created = await _service.CreateAsync(category, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, Map(created));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CategoryResponse>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var category = await _service.GetByIdAsync(id, cancellationToken);
        if (category is null) return NotFound();
        return Ok(Map(category));
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<CategoryResponse>>> GetAll(CancellationToken cancellationToken)
    {
        var categories = await _service.GetAllAsync(cancellationToken);
        return Ok(categories.Select(Map).ToList());
    }

    private static CategoryResponse Map(Category c) =>
        new(c.Id, c.Name, c.Description, c.RequiredZone, c.CreatedAtUtc);
}