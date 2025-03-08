namespace Playground.EventSourcing.Aggregates;

public record OrderPlaced(Guid OrderId, string ProductName, int Quantity) : IEvent;
public record OrderShipped(Guid OrderId, DateTime ShippedDate) : IEvent;
public record OrderCompleted(Guid OrderId, DateTime CompletedDate) : IEvent;

public class OrderAggregate
{
    public Guid Id { get; private set; }
    public string ProductName { get; private set; }
    public int Quantity { get; private set; }
    public DateTimeOffset? ShippedDate { get; private set; }
    public DateTimeOffset? CompletedDate { get; private set; }
    public bool IsCompleted => CompletedDate.HasValue;

    // Zustand durch Events aufbauen
    public void Apply(OrderPlaced @event)
    {
        Id = @event.OrderId;
        ProductName = @event.ProductName;
        Quantity = @event.Quantity;
    }

    public void Apply(OrderShipped @event)
    {
        ShippedDate = @event.ShippedDate;
    }

    public void Apply(OrderCompleted @event)
    {
        CompletedDate = @event.CompletedDate;
    }

    // Events anwenden
    public void ApplyEvent(object @event)
    {
        switch (@event)
        {
            case OrderPlaced placed: Apply(placed); break;
            case OrderShipped shipped: Apply(shipped); break;
            case OrderCompleted completed: Apply(completed); break;
        }
    }
}