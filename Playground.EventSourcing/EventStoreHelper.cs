using Marten;
using Microsoft.Extensions.Configuration;
using Weasel.Core;

namespace Playground.EventSourcing;

public static class EventStoreHelper
{
    public static DocumentStore SetupDocumentStore(string connectionString)
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
    
    public static string ConnectionStringOrThrow()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<Program>()
            .Build();

        var connectionString = config["ConnectionStrings:Postgres"]
                               ?? throw new InvalidOperationException("Database connection string is missing.");
        return connectionString;
    }
}