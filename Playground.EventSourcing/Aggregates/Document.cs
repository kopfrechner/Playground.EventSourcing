using Playground.EventSourcing.Aggregates.Common;

namespace Playground.EventSourcing.Aggregates;

public record DocumentAdded(Guid DocumentId, string Alias, DateTimeOffset CreatedAt, string CreatedBy) : IDomainEvent;
public record CandidateFileRevisionUploaded(Guid DocumentId, string FileName, string InternalUniqueFileName, string ChangeLog, DateTimeOffset UploadedAt, string UploadedBy) : IDomainEvent;
public record CandidateFileRevisionApproved(Guid DocumentId, string FileName, string InternalUniqueFileName, string ChangeLog, string ApprovalComment, DateTimeOffset ApprovedAt, string ApprovedBy) : IDomainEvent;
public record CandidateFileRevisionDeclined(Guid DocumentId, string DeclinedComment, DateTimeOffset DeclinedAt, string DeclinedBy) : IDomainEvent;
public record DocumentLocked(Guid DocumentId, string LockReason, DateTimeOffset LockedAt, string LockedBy) : IDomainEvent;
public record DocumentUnlocked(Guid DocumentId, string UnlockReason, DateTimeOffset UnlockedAt, string UnlockedBy) : IDomainEvent;

public sealed record FileRevision(
    string FileName,
    string InternalUniqueFileName,
    int Revision,
    string ChangeLog,
    DateTimeOffset ApprovedAt,
    string ApprovedBy
);

public sealed record Document(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    bool OngoingApprovalProcess,
    bool IsLocked,
    string? LockChangedReason,
    DateTimeOffset? LockedChangedAt,
    int CurrentVersion,
    FileRevision? CurrentFileRevision,
    IReadOnlyList<FileRevision> Revisions) : AggregateBase<Document>(Id)
{
    public static Document Create(DocumentAdded added) => new(
        added.DocumentId,
        added.Alias,
        added.CreatedAt,
        added.CreatedBy,
        IsLocked: false,
        LockChangedReason: null,
        LockedChangedAt: null,
        CurrentVersion: 0,
        CurrentFileRevision: null,
        OngoingApprovalProcess: false,
        Revisions: []);
    
    protected override Document ApplyEventInternal(IDomainEvent @event)=> @event switch
    {
        CandidateFileRevisionUploaded uploaded => Apply(uploaded),
        CandidateFileRevisionApproved approved => Apply(approved),
        CandidateFileRevisionDeclined declined => Apply(declined),
        DocumentLocked locked => Apply(locked),
        DocumentUnlocked unlocked => Apply(unlocked),
        _ => throw new InvalidOperationException($"Unsupported event type: {@event.GetType().Name}")
    };
    
    private Document Apply(CandidateFileRevisionUploaded _)
    {
        if (OngoingApprovalProcess)
        {
            throw new InvalidOperationException();
        }
        
        return this with
        {
            OngoingApprovalProcess = true
        };
    }
    
    private Document Apply(CandidateFileRevisionApproved @event)
    {
        if (!OngoingApprovalProcess)
        {
            throw new InvalidOperationException("To add a file revision, there must be an ongoing approval process.");
        }
        
        var nextVersion = CurrentVersion + 1;
        var newFileRevision = new FileRevision(
            @event.FileName,
            @event.InternalUniqueFileName,
            nextVersion,
            @event.ChangeLog,
            @event.ApprovedAt,
            @event.ApprovedBy);

        return this with
        {
            OngoingApprovalProcess = false,
            CurrentVersion = nextVersion,
            CurrentFileRevision = newFileRevision,
            Revisions = [.. Revisions, newFileRevision]
        };
    }
    
    private Document Apply(CandidateFileRevisionDeclined _)
    {
        if (!OngoingApprovalProcess)
        {
            throw new InvalidOperationException();
        }
        
        return this with
        {
            OngoingApprovalProcess = false
        };
    }
    
    private Document Apply(DocumentLocked locked)
    {
        return this with
        {
            IsLocked = true,
            LockChangedReason = locked.LockReason,
            LockedChangedAt = locked.LockedAt
        };
    }
    
    private Document Apply(DocumentUnlocked unlocked)
    {
        return this with
        {
            IsLocked = false,
            LockChangedReason = unlocked.UnlockReason,
            LockedChangedAt = unlocked.UnlockedAt
        };
    }
}