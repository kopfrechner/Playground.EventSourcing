using Playground.EventSourcing;

// Configure Marten
var connectionString = EventStoreHelper.ConnectionStringOrThrow();
var store = EventStoreHelper.SetupDocumentStore(connectionString);

await using var session = store.LightweightSession();

var orderId = Guid.NewGuid();

// Events
session.Events.StartStream(orderId,
    new OrderPlaced(orderId, "Laptop", 2),
    new OrderShipped(orderId, DateTime.UtcNow),
    new OrderCompleted(orderId, DateTime.UtcNow.AddHours(1))
);
await session.SaveChangesAsync();

// Manually with FetchStream
var events = await session.Events.FetchStreamAsync(orderId);
var order = new OrderAggregate();
foreach (var e in events) order.ApplyEvent(e.Data);
Console.WriteLine($"Order: {order.ProductName}, Shipped: {order.ShippedDate}, Completed: {order.IsCompleted}");

// Try to get aggregate with marten and versioning
var orderAggregateV1 = await session.Events.AggregateStreamAsync<OrderAggregate>(orderId, 1);
Console.WriteLine($"Order: {orderAggregateV1!.ProductName}, Shipped: {orderAggregateV1.ShippedDate}, Completed: {orderAggregateV1.IsCompleted}");

var orderAggregateV2 = await session.Events.AggregateStreamAsync<OrderAggregate>(orderId, 2);
Console.WriteLine($"Order: {orderAggregateV2!.ProductName}, Shipped: {orderAggregateV2.ShippedDate}, Completed: {orderAggregateV2.IsCompleted}");

var orderAggregate = await session.Events.AggregateStreamAsync<OrderAggregate>(orderId);
Console.WriteLine($"Order: {orderAggregate!.ProductName}, Shipped: {orderAggregate.ShippedDate}, Completed: {orderAggregate.IsCompleted}");

