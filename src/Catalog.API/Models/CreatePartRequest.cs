namespace Catalog.API.Models;

public record CreatePartRequest(
    string Sku,
    string Name,
    PartCategory Category,
    string? Description,
    decimal UnitPrice,
    int StockQuantity);
