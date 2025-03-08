using Marten.Events.Projections;

namespace Playground.EventSourcing.Aggregates.Projections;

public record CandidateFileRevisionHistoryEntry(
    Guid DocumentId,
    string? FileName,
    string? InternalUniqueFileName,
    string? ChangeLog,
    DateTimeOffset EventTime,
    FileRevisionApprovalStatus Status,
    string? ChangeComment,
    string PerformedBy
)
{
    public Guid Id { get; private set; }
};

public enum FileRevisionApprovalStatus
{
    Pending,
    Approved,
    Declined
}

public class CandidateFileRevisionHistoryProjection : MultiStreamProjection<CandidateFileRevisionHistoryEntry, Guid>
{
    public CandidateFileRevisionHistoryProjection()
    {
        Identity<CandidateFileRevisionUploaded>(_ => Guid.NewGuid());
        Identity<CandidateFileRevisionApproved>(_ => Guid.NewGuid());
        Identity<CandidateFileRevisionDeclined>(_ => Guid.NewGuid());
    }

    public CandidateFileRevisionHistoryEntry Create(CandidateFileRevisionUploaded @event) =>
        new(@event.DocumentId,
            @event.FileName,
            @event.InternalUniqueFileName,
            @event.ChangeLog,
            @event.UploadedAt,
            FileRevisionApprovalStatus.Pending,
            null,
            @event.UploadedBy);

    public CandidateFileRevisionHistoryEntry Create(CandidateFileRevisionApproved @event) =>
        new(@event.DocumentId,
            @event.FileName,
            @event.InternalUniqueFileName,
            @event.ChangeLog,
            @event.ApprovedAt,
            FileRevisionApprovalStatus.Approved,
            @event.ApprovalComment,
            @event.ApprovedBy);

    public CandidateFileRevisionHistoryEntry Create(CandidateFileRevisionDeclined @event) =>
        new(@event.DocumentId,
            FileName: null,
            InternalUniqueFileName: null,
            ChangeLog: null,
            @event.DeclinedAt,
            FileRevisionApprovalStatus.Declined,
            @event.DeclinedComment,
            @event.DeclinedBy);
}