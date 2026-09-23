using System.Net;
using System.Text.Json.Serialization;
using Orders.API.Data;
using Orders.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Serialize enums as strings ("Placed") instead of numbers (0).
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

// Where the catalog service lives. Overridden in docker-compose via
// the CatalogApi__BaseUrl environment variable.
var catalogBaseUrl = builder.Configuration["CatalogApi:BaseUrl"] ?? "http://localhost:5001";
builder.Services.AddHttpClient("catalog", client =>
{
    client.BaseAddress = new Uri(catalogBaseUrl);
    client.Timeout = TimeSpan.FromSeconds(10);
});

var app = builder.Build();

app.MapGet("/healthz", () => Results.Ok(new { service = "orders", status = "healthy" }));

app.MapGet("/orders", () =>
    Results.Ok(OrdersStore.Orders.Values.OrderByDescending(o => o.CreatedAt)))
.WithName("ListOrders");

app.MapGet("/orders/{id}", (string id) =>
    OrdersStore.Orders.TryGetValue(id, out var order)
        ? Results.Ok(order)
        : Results.NotFound(new { error = $"Order '{id}' was not found." }))
.WithName("GetOrder");

app.MapPost("/orders", async (CreateOrderRequest request, IHttpClientFactory httpClientFactory) =>
{
    if (string.IsNullOrWhiteSpace(request.PartId))
    {
        return Results.BadRequest(new { error = "PartId is required." });
    }
    if (request.Quantity <= 0)
    {
        return Results.BadRequest(new { error = "Quantity must be greater than zero." });
    }
    if (string.IsNullOrWhiteSpace(request.CustomerName))
    {
        return Results.BadRequest(new { error = "CustomerName is required." });
    }

    // Cross-service validation: the part must exist in the catalog.
    var catalog = httpClientFactory.CreateClient("catalog");

    CatalogPartDto? part;
    try
    {
        using var response = await catalog.GetAsync($"/parts/{Uri.EscapeDataString(request.PartId)}");

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return Results.NotFound(new { error = $"Part '{request.PartId}' does not exist in the catalog." });
        }

        response.EnsureSuccessStatusCode();
        part = await response.Content.ReadFromJsonAsync<CatalogPartDto>();
    }
    catch (HttpRequestException ex)
    {
        return Results.Problem(
            title: "Catalog service unavailable",
            detail: $"Could not reach the catalog service: {ex.Message}",
            statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    if (part is null)
    {
        return Results.Problem(
            title: "Catalog service error",
            detail: "The catalog service returned an unexpected response.",
            statusCode: StatusCodes.Status502BadGateway);
    }

    if (part.StockQuantity < request.Quantity)
    {
        return Results.Conflict(new
        {
            error = $"Insufficient stock for part '{part.Sku}'. Requested {request.Quantity}, available {part.StockQuantity}."
        });
    }

    var now = DateTimeOffset.UtcNow;
    var order = new Order(
        Id: Guid.NewGuid().ToString("N")[..12],
        OrderNumber: $"PO-{now:yyyyMMdd}-{Random.Shared.Next(1000, 10000)}",
        PartId: part.Id,
        PartSku: part.Sku,
        PartName: part.Name,
        Quantity: request.Quantity,
        UnitPrice: part.UnitPrice,
        TotalPrice: part.UnitPrice * request.Quantity,
        CustomerName: request.CustomerName.Trim(),
        Status: OrderStatus.Placed,
        CreatedAt: now);

    OrdersStore.Orders.TryAdd(order.Id, order);

    return Results.Created($"/orders/{order.Id}", order);
})
.WithName("CreateOrder");

app.Run();
