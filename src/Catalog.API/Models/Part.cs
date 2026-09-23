namespace Catalog.API.Models;

public enum PartCategory
{
    Coupling,
    Bearing,
    Gear,
    Fastener,
    Belt,
    Seal
}

public record Part(
    string Id,
    string Sku,
    string Name,
    PartCategory Category,
    string Description,
    decimal UnitPrice,
    int StockQuantity);
