using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Api.Contracts;

public record CreateProductRequest(
    [Required, MaxLength(200)] string Name,
    [Required, MaxLength(50)] string Sku,
    [MaxLength(2000)] string? Description,
    [Range(0.01, 1_000_000)] decimal Price,
    [Range(0, int.MaxValue)] int StockQuantity
);