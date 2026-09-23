namespace Orders.API.Models;

/// <summary>
/// The subset of the catalog's Part shape that Orders.API needs
/// when validating an order against Catalog.API.
/// </summary>
public record CatalogPartDto(
    string Id,
    string Sku,
    string Name,
    decimal UnitPrice,
    int StockQuantity);
