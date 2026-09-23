using System.Collections.Concurrent;
using System.Text.Json.Serialization;
using Catalog.API.Data;
using Catalog.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as strings ("Coupling") instead of numbers (0).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

var parts = new ConcurrentDictionary<string, Part>(
    SeedData.Parts.ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase));

app.MapGet("/healthz", () => Results.Ok(new { service = "catalog", status = "healthy" }));

app.MapGet("/parts", (string? category) =>
{
    IEnumerable<Part> result = parts.Values;

    if (!string.IsNullOrWhiteSpace(category))
    {
        result = result.Where(p =>
            p.Category.ToString().Contains(category, StringComparison.OrdinalIgnoreCase));
    }

    return Results.Ok(result.OrderBy(p => p.Sku));
})
.WithName("ListParts");

app.MapGet("/parts/search", (string? q) =>
{
    if (string.IsNullOrWhiteSpace(q))
    {
        return Results.BadRequest(new { error = "Query string parameter 'q' is required." });
    }

    var matches = parts.Values.Where(p =>
        p.Name.Contains(q, StringComparison.OrdinalIgnoreCase) ||
        p.Sku.Contains(q, StringComparison.OrdinalIgnoreCase) ||
        p.Description.Contains(q, StringComparison.OrdinalIgnoreCase));

    return Results.Ok(matches.OrderBy(p => p.Sku));
})
.WithName("SearchParts");

app.MapGet("/parts/{id}", (string id) =>
    parts.TryGetValue(id, out var part)
        ? Results.Ok(part)
        : Results.NotFound(new { error = $"Part '{id}' was not found." }))
.WithName("GetPart");

app.MapPost("/parts", (CreatePartRequest request) =>
{
    var errors = new List<string>();
    if (string.IsNullOrWhiteSpace(request.Sku))
    {
        errors.Add("Sku is required.");
    }
    if (string.IsNullOrWhiteSpace(request.Name))
    {
        errors.Add("Name is required.");
    }
    if (request.UnitPrice < 0)
    {
        errors.Add("UnitPrice cannot be negative.");
    }
    if (request.StockQuantity < 0)
    {
        errors.Add("StockQuantity cannot be negative.");
    }

    if (errors.Count > 0)
    {
        return Results.BadRequest(new { errors });
    }

    var id = Slugify(request.Sku);
    var part = new Part(
        id,
        request.Sku.Trim(),
        request.Name.Trim(),
        request.Category,
        request.Description ?? string.Empty,
        request.UnitPrice,
        request.StockQuantity);

    if (!parts.TryAdd(id, part))
    {
        return Results.Conflict(new { error = $"A part with SKU '{request.Sku}' already exists." });
    }

    return Results.Created($"/parts/{id}", part);
})
.WithName("CreatePart");

app.Run();

static string Slugify(string value)
{
    var slug = new string(value.Trim().ToLowerInvariant()
        .Select(c => char.IsLetterOrDigit(c) ? c : '-')
        .ToArray());

    slug = string.Join("-", slug.Split('-', StringSplitOptions.RemoveEmptyEntries));

    return string.IsNullOrEmpty(slug)
        ? Guid.NewGuid().ToString("N")[..8]
        : slug;
}
