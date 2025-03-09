using Marten.Events;
using Playground.EventSourcing.Aggregates.Common;

namespace Playground.EventSourcing.Aggregates;

public abstract record AggregateBase<T>(Guid Id) where T : AggregateBase<T> 
{
    private readonly List<IDomainEvent> _uncommittedEvents = [];
    public IReadOnlyList<IDomainEvent> UncommittedEvents => _uncommittedEvents.AsReadOnly();

    public T ApplyEvent(IDomainEvent @event)
    {
        var aggregate = ApplyEventInternal(@event);
        _uncommittedEvents.Add(@event);
        return aggregate;
    }
    
    public T ApplyEvents(params IList<IDomainEvent> events)
    {
        var aggregate = this as T;
        foreach (var @event in events)
        {
            aggregate = aggregate!.ApplyEvent(@event);
        }

        return aggregate!;
    }

    protected abstract T ApplyEventInternal(IDomainEvent @event);
    
    public void ClearUncommitedEvents() => _uncommittedEvents.Clear();
}

public static class AggregateRootExtensions
{
    public static void AppendAndClearUncommitedEvents<T>(this IEventStore eventStore, T aggregate) where T : AggregateBase<T>
    {
        eventStore.Append(aggregate.Id, aggregate.UncommittedEvents);
        aggregate.ClearUncommitedEvents();
    }
}