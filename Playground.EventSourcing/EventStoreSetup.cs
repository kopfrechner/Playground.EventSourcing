using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Playground.EventSourcing.Aggregates;
using Weasel.Core;

namespace Playground.EventSourcing;

public static class EventStoreSetup
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
            options.Projections.Add<CandidateFileRevisionProjection>(ProjectionLifecycle.Inline);
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

internal abstract class Program;