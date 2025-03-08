using Marten.Events.Projections;

namespace Playground.EventSourcing.Aggregates.Projections;

public record ProductRevisionHistoryEntry(
    Guid ProductId,
    string? Description,
    string? InternalDescription,
    string? ChangeLog,
    DateTimeOffset EventTime,
    ProductRevisionApprovalStatus Status,
    string? ChangeComment,
    string PerformedBy
)
{
    public Guid Id { get; private set; }
};

public enum ProductRevisionApprovalStatus
{
    Pending,
    Approved,
    Declined
}

public class ProductRevisionHistoryProjection : MultiStreamProjection<ProductRevisionHistoryEntry, Guid>
{
    public ProductRevisionHistoryProjection()
    {
        Identity<ProductRevisionUploaded>(_ => Guid.NewGuid());
        Identity<ProductRevisionApproved>(_ => Guid.NewGuid());
        Identity<ProductRevisionDeclined>(_ => Guid.NewGuid());
    }

    public ProductRevisionHistoryEntry Create(ProductRevisionUploaded @event) =>
        new(@event.ProductId,
            @event.Description,
            @event.InternalDescription,
            @event.ChangeLog,
            @event.UploadedAt,
            ProductRevisionApprovalStatus.Pending,
            null,
            @event.UploadedBy);

    public ProductRevisionHistoryEntry Create(ProductRevisionApproved @event) =>
        new(@event.ProductId,
            @event.Description,
            @event.InternalDescription,
            @event.ChangeLog,
            @event.ApprovedAt,
            ProductRevisionApprovalStatus.Approved,
            @event.ApprovalComment,
            @event.ApprovedBy);

    public ProductRevisionHistoryEntry Create(ProductRevisionDeclined @event) =>
        new(@event.ProductId,
            Description: null,
            InternalDescription: null,
            ChangeLog: null,
            @event.DeclinedAt,
            ProductRevisionApprovalStatus.Declined,
            @event.DeclinedComment,
            @event.DeclinedBy);
}