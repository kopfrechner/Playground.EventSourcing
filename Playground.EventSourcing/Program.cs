using Marten;
using Microsoft.Extensions.Configuration;
using Playground.EventSourcing;
using Weasel.Core;

// Load configuration from user secrets
var connectionString = ConnectionStringOrThrow();

// Configure Marten
var store = SetupDocumentStore(connectionString);

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

static DocumentStore SetupDocumentStore(string connectionString)
{
    return DocumentStore.For(options =>
    {
        options.DatabaseSchemaName = "playgound";
        options.Connection(connectionString);
        options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
        
        var eventTypes = typeof(IEvent).Assembly.GetTypes()
            .Where(t => typeof(IEvent).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
            .ToList();

        foreach (var type in eventTypes)
        {
            options.Events.AddEventType(type);
        }
    });
}

static string ConnectionStringOrThrow()
{
    var config = new ConfigurationBuilder()
        .AddUserSecrets<Program>()
        .Build();

    var connectionString = config["ConnectionStrings:Postgres"]
                           ?? throw new InvalidOperationException("Database connection string is missing.");
    return connectionString;
}