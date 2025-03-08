using Marten.Events.Projections;

namespace Playground.EventSourcing.Aggregates.Projections;

public record CandidateProductRevisionHistoryEntry(
    Guid ProductId,
    string? Description,
    string? InternalUniqueDescription,
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

public class CandidateProductRevisionHistoryProjection : MultiStreamProjection<CandidateProductRevisionHistoryEntry, Guid>
{
    public CandidateProductRevisionHistoryProjection()
    {
        Identity<CandidateProductRevisionUploaded>(_ => Guid.NewGuid());
        Identity<CandidateProductRevisionApproved>(_ => Guid.NewGuid());
        Identity<CandidateProductRevisionDeclined>(_ => Guid.NewGuid());
    }

    public CandidateProductRevisionHistoryEntry Create(CandidateProductRevisionUploaded @event) =>
        new(@event.ProductId,
            @event.Description,
            @event.InternalUniqueDescription,
            @event.ChangeLog,
            @event.UploadedAt,
            ProductRevisionApprovalStatus.Pending,
            null,
            @event.UploadedBy);

    public CandidateProductRevisionHistoryEntry Create(CandidateProductRevisionApproved @event) =>
        new(@event.ProductId,
            @event.Description,
            @event.InternalUniqueDescription,
            @event.ChangeLog,
            @event.ApprovedAt,
            ProductRevisionApprovalStatus.Approved,
            @event.ApprovalComment,
            @event.ApprovedBy);

    public CandidateProductRevisionHistoryEntry Create(CandidateProductRevisionDeclined @event) =>
        new(@event.ProductId,
            Description: null,
            InternalUniqueDescription: null,
            ChangeLog: null,
            @event.DeclinedAt,
            ProductRevisionApprovalStatus.Declined,
            @event.DeclinedComment,
            @event.DeclinedBy);
}