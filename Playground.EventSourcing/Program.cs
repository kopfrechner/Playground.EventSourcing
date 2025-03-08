using Bogus;
using Marten;
using Playground.EventSourcing;
using Playground.EventSourcing.Aggregates;

// Configure Marten
var connectionString = EventStoreSetup.ConnectionStringOrThrow();
var store = EventStoreSetup.SetupDocumentStore(connectionString);

await using var session = store.LightweightSession();

// Events
await DocumentPlayground(session);
//await OrderPlayGround(session);

async Task DocumentPlayground(IDocumentSession documentSession)
{
    var documentId1 = Guid.NewGuid();
    
    Randomizer.Seed = new Random(420);
    
    documentSession.Events.StartStream<Document>(documentId1,
        Fake.DocumentAdded(documentId1),
        Fake.FileRevisionApproved(documentId1),
        Fake.FileRevisionApproved(documentId1),
        Fake.FileRevisionApproved(documentId1)
    );
    
    var documentId2 = Guid.NewGuid();
    documentSession.Events.StartStream<Document>(documentId2,
        Fake.DocumentAdded(documentId2),
        Fake.FileRevisionApproved(documentId2),
        Fake.FileRevisionApproved(documentId2)
    );
    await documentSession.SaveChangesAsync();
    
    var documentV1 = await documentSession.Events.AggregateStreamAsync<Document>(documentId1);
    Console.WriteLine(documentV1.ToPrettyJson());
    
    var documentV2 = await documentSession.Events.AggregateStreamAsync<Document>(documentId2);
    Console.WriteLine(documentV2.ToPrettyJson());
}

async Task OrderPlayGround(IDocumentSession documentSession)
{
    var orderId = Guid.NewGuid();
    
    documentSession.Events.StartStream(orderId,
        new OrderPlaced(orderId, "Laptop", 2),
        new OrderShipped(orderId, DateTime.UtcNow),
        new OrderCompleted(orderId, DateTime.UtcNow.AddHours(1))
    );
    await documentSession.SaveChangesAsync();

    // Manually with FetchStream
    var events = await documentSession.Events.FetchStreamAsync(orderId);
    var order = new OrderAggregate();
    foreach (var e in events) order.ApplyEvent(e.Data);
    Console.WriteLine($"Order: {order.ProductName}, Shipped: {order.ShippedDate}, Completed: {order.IsCompleted}");

    // Try to get aggregate with marten and versioning
    var orderAggregateV1 = await documentSession.Events.AggregateStreamAsync<OrderAggregate>(orderId, 1);
    Console.WriteLine($"Order: {orderAggregateV1!.ProductName}, Shipped: {orderAggregateV1.ShippedDate}, Completed: {orderAggregateV1.IsCompleted}");

    var orderAggregateV2 = await documentSession.Events.AggregateStreamAsync<OrderAggregate>(orderId, 2);
    Console.WriteLine($"Order: {orderAggregateV2!.ProductName}, Shipped: {orderAggregateV2.ShippedDate}, Completed: {orderAggregateV2.IsCompleted}");

    var orderAggregate = await documentSession.Events.AggregateStreamAsync<OrderAggregate>(orderId);
    Console.WriteLine($"Order: {orderAggregate!.ProductName}, Shipped: {orderAggregate.ShippedDate}, Completed: {orderAggregate.IsCompleted}");
}

