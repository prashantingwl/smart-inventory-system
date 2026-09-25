using System.ComponentModel.DataAnnotations;
using SmartInventory.Core.Entities;

namespace SmartInventory.Api.Contracts;

public record CreateWarehouseRequest(
    [Required]
    [StringLength(200, MinimumLength = 2)]
    string Name,

    [Required]
    [StringLength(50, MinimumLength = 2)]
    [RegularExpression(@"^[A-Z0-9\-]{2,50}$",
        ErrorMessage = "Warehouse code must be uppercase letters, digits, or hyphens.")]
    string Code,

    [StringLength(500)]
    string? AddressLine,

    [StringLength(100)]
    string? City,

    [StringLength(20)]
    string? PinCode,

    [Required]
    TemperatureZone SupportedZone
);

public record WarehouseResponse(
    Guid Id,
    string Name,
    string Code,
    string? AddressLine,
    string? City,
    string? PinCode,
    TemperatureZone SupportedZone,
    bool IsActive,
    DateTime CreatedAtUtc
);