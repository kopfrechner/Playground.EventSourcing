using Playground.EventSourcing;

// Load configuration from user secrets
var connectionString = EventStoreHelper.ConnectionStringOrThrow();

// Configure Marten
var store = EventStoreHelper.SetupDocumentStore(connectionString);

await using var session = store.LightweightSession();

var orderId = Guid.NewGuid();

// Stream starten mit Events
session.Events.StartStream(orderId,
    new OrderPlaced(orderId, "Laptop", 2),
    new OrderShipped(orderId, DateTime.UtcNow),
    new OrderCompleted(orderId, DateTime.UtcNow.AddHours(1))
);
await session.SaveChangesAsync();

// Aggregate rekonstruieren
var events = await session.Events.FetchStreamAsync(orderId);
var order = new OrderAggregate();
foreach (var e in events) order.ApplyEvent(e.Data);

Console.WriteLine($"Order: {order.ProductName}, Shipped: {order.ShippedDate}, Completed: {order.IsCompleted}");