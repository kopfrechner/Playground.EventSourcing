using Marten;
using Marten.Events.Projections;
using Playground.EventSourcing.Aggregates.Common;
using Playground.EventSourcing.Aggregates.Projections;
using Weasel.Core;

namespace Playground.EventSourcing.Tests.Common;

public static class EventStoreSetup
{
    public static DocumentStore SetupProductStore(string connectionString)
    {
        return DocumentStore.For(options =>
        {
            options.DatabaseSchemaName = "playgound";
            options.Connection(connectionString);
            options.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
        
            var eventTypes = typeof(IDomainEvent).Assembly.GetTypes()
                .Where(t => typeof(IDomainEvent).IsAssignableFrom(t) && t is { IsClass: true, IsAbstract: false })
                .ToList();

            foreach (var type in eventTypes)
            {
                options.Events.AddEventType(type);
            }
            
            options.Projections.Add<ProductRevisionToReviewProjection>(ProjectionLifecycle.Inline);
            options.Projections.Add<ProductRevisionHistoryProjection>(ProjectionLifecycle.Inline);
        });
    }
}