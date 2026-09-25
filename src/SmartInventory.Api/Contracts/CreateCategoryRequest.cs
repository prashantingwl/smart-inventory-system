using System.ComponentModel.DataAnnotations;
using SmartInventory.Core.Entities;

namespace SmartInventory.Api.Contracts;

public record CreateCategoryRequest(
    [Required]
    [StringLength(200, MinimumLength = 2)]
    string Name,

    [StringLength(1000)]
    string? Description,

    [Required]
    TemperatureZone RequiredZone
);

public record CategoryResponse(
    Guid Id,
    string Name,
    string? Description,
    TemperatureZone RequiredZone,
    DateTime CreatedAtUtc
);