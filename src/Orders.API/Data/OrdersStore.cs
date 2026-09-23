using System.Collections.Concurrent;
using Orders.API.Models;

namespace Orders.API.Data;

public static class OrdersStore
{
    public static ConcurrentDictionary<string, Order> Orders { get; } =
        new(StringComparer.OrdinalIgnoreCase);
}
