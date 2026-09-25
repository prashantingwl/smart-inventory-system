using System.ComponentModel.DataAnnotations;

namespace SmartInventory.Api.Contracts;

public record CreateProductRequest(
    [Required] Guid CategoryId,
    [Required]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be 2–200 characters.")]
    [RegularExpression(@"^[A-Za-z0-9\s\-\.&']+$",
        ErrorMessage = "Name contains invalid characters.")]
    string Name,

    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "SKU must be 3–50 characters.")]
    [RegularExpression(@"^[A-Z]{2,4}-[A-Z0-9]{1,20}-\d{3,6}$",
        ErrorMessage = "SKU must follow format: XX-XXXX-000 (e.g., KB-MECH-001).")]
    string Sku,

    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be 10–2000 characters if provided.")]
    string? Description,

    [Range(typeof(decimal), "0.01", "100000", ErrorMessage = "Price must be between 0.01 and 100,000.")]
    decimal Price,

    [Range(0, 100000, ErrorMessage = "Stock quantity must be between 0 and 100,000.")]
    int StockQuantity
);