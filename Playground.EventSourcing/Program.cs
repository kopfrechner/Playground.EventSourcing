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
    var documentId = Guid.NewGuid();
    documentSession.Events.StartStream<Document>(documentId,
        new DocumentAdded(documentId, "Product Name", DateTimeOffset.Now, "Norbert Normalo"),
        new FileRevisionApproved(documentId, "ProductDescription.pdf", $"{Guid.NewGuid()}.pdf", DateTimeOffset.Now, "Hans Approver"),
        new FileRevisionApproved(documentId, "ProductDescription_vnext.pdf", $"{Guid.NewGuid()}.pdf", DateTimeOffset.Now, "Claudia Approver"),
        new FileRevisionApproved(documentId, "ProductDescription_v3.pdf", $"{Guid.NewGuid()}.pdf", DateTimeOffset.Now, "Werner Approver")
    );
    await documentSession.SaveChangesAsync();
    
    var documentV1 = await documentSession.Events.AggregateStreamAsync<Document>(documentId);
    Console.WriteLine(documentV1.ToPrettyJson());

    for (var i = 0; i < 4; i++)
    {
        var documentVx = await documentSession.Events.AggregateStreamAsync<Document>(documentId, i);
        Console.WriteLine(documentVx.ToPrettyJson());
    }
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

