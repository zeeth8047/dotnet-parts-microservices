using Catalog.API.Models;

namespace Catalog.API.Data;

/// <summary>
/// Initial catalog contents. A small homage to the mechanical-engineering
/// side of things: couplings, bearings, gears and friends.
/// </summary>
public static class SeedData
{
    public static IReadOnlyList<Part> Parts { get; } = new List<Part>
    {
        new(
            Id: "jaw-coupling-l095",
            Sku: "JAW-L095-CI",
            Name: "Jaw Coupling - L095 with Spider Insert",
            Category: PartCategory.Coupling,
            Description: "Three-piece jaw-type flexible coupling: cast-iron hubs with a Buna-N spider insert. Rated 180 in-lb.",
            UnitPrice: 24.99m,
            StockQuantity: 140),

        new(
            Id: "bearing-6205-2rs",
            Sku: "BRG-6205-2RS",
            Name: "Deep Groove Ball Bearing 6205-2RS",
            Category: PartCategory.Bearing,
            Description: "Single-row deep groove ball bearing, 25 x 52 x 15 mm, rubber sealed both sides.",
            UnitPrice: 8.49m,
            StockQuantity: 320),

        new(
            Id: "spur-gear-20t-m2",
            Sku: "GEAR-SPUR-20T-M2",
            Name: "Spur Gear - 20 Teeth, Module 2",
            Category: PartCategory.Gear,
            Description: "Steel spur gear, 20 teeth, module 2, 20 mm face width, 12 mm bore with keyway.",
            UnitPrice: 18.75m,
            StockQuantity: 85),

        new(
            Id: "flange-coupling-f100",
            Sku: "CPLG-FLN-F100",
            Name: "Flanged Rigid Coupling F100",
            Category: PartCategory.Coupling,
            Description: "Rigid flanged coupling for 28 mm shafts, cast steel, mounting bolts included.",
            UnitPrice: 42.00m,
            StockQuantity: 60),

        new(
            Id: "hex-bolt-m12x50",
            Sku: "FAST-HHB-M12X50",
            Name: "Hex Head Bolt M12 x 50 - Class 8.8",
            Category: PartCategory.Fastener,
            Description: "Hex head cap screw, M12 x 1.75 x 50 mm, property class 8.8, zinc plated.",
            UnitPrice: 0.85m,
            StockQuantity: 2500),

        new(
            Id: "v-belt-b60",
            Sku: "BELT-V-B60",
            Name: "V-Belt - B Section, 1524 mm",
            Category: PartCategory.Belt,
            Description: "Classical wrapped V-belt, B section (17 x 11 mm), 60 in / 1524 mm outside length.",
            UnitPrice: 12.30m,
            StockQuantity: 190),
    }.AsReadOnly();
}
