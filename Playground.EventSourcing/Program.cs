using Marten;
using Microsoft.Extensions.Configuration;

// Define an event
public record OrderPlaced(Guid OrderId, string ProductName, int Quantity);

class Program
{
    static async Task Main()
    {
        // Load configuration from user secrets
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();
        
        var connectionString = config["ConnectionStrings:Postgres"]
                               ?? throw new InvalidOperationException("Database connection string is missing.");
        
        // Configure Marten
        var store = DocumentStore.For(options =>
        {
            options.Connection(connectionString);
            options.Events.AddEventType(typeof(OrderPlaced));
        });

        using var session = store.OpenSession();
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
}