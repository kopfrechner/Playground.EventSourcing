using Marten;
using Microsoft.Extensions.Configuration;
using Weasel.Core;


// Define an event

public interface IEvent { }


public record OrderPlaced(Guid OrderId, string ProductName, int Quantity);

class Program
{
    static async Task Main()
    {
        // Load configuration from user secrets
        var connectionString = ConnectionStringOrThrow();

        // Configure Marten
        var store = SetupDocumentStore(connectionString);

        await using var session = store.LightweightSession();
        var orderId = Guid.NewGuid();
        
        // Store an event
        session.Events.StartStream<OrderPlaced>(orderId, new OrderPlaced(orderId, "Laptop", 1));
        await session.SaveChangesAsync();
        
        // Retrieve and display events
        var events = await session.Events.FetchStreamAsync(orderId);
        foreach (var @event in events)
        {
            Console.WriteLine(@event.Data);
        }
    }

    private static DocumentStore SetupDocumentStore(string connectionString)
    {
        return DocumentStore.For(options =>
        {
            options.DatabaseSchemaName = "playgound";
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.None;
            
            var eventTypes = typeof(IEvent).Assembly.GetTypes()
                .Where(t => typeof(IEvent).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
                .ToList();

            foreach (var type in eventTypes)
            {
                options.Events.AddEventType(type);
            }
        });
    }

    private static string ConnectionStringOrThrow()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString = config["ConnectionStrings:Postgres"]
                               ?? throw new InvalidOperationException("Database connection string is missing.");
        return connectionString;
    }
}