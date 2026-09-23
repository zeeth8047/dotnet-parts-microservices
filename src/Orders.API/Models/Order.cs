namespace Orders.API.Models;

public enum OrderStatus
{
    Placed,
    Confirmed,
    Shipped,
    Cancelled
}

public record Order(
    string Id,
    string OrderNumber,
    string PartId,
    string PartSku,
    string PartName,
    int Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    string CustomerName,
    OrderStatus Status,
    DateTimeOffset CreatedAt);

public record CreateOrderRequest(
    string PartId,
    int Quantity,
    string CustomerName);
