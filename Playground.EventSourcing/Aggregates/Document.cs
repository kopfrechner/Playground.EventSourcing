using Marten.Events;
using Playground.EventSourcing.Aggregates.Common;

namespace Playground.EventSourcing.Aggregates;

public record DocumentAdded(Guid DocumentId, string Alias, DateTimeOffset CreatedAt, string CreatedBy) : IDomainEvent;
public record CandidateFileRevisionUploaded(Guid DocumentId, string FileName, string InternalUniqueFileName, string ChangeLog, DateTimeOffset UploadedAt, string UploadedBy) : IDomainEvent;
public record CandidateFileRevisionApproved(Guid DocumentId, string FileName, string InternalUniqueFileName, string ChangeLog, string ApprovalComment, DateTimeOffset ApprovedAt, string ApprovedBy) : IDomainEvent;
public record CandidateFileRevisionDeclined(Guid DocumentId, string DeclinedComment, DateTimeOffset DeclinedAt, string DeclinedBy) : IDomainEvent;

//public record DocumentLocked(Guid DocumentId, DateTimeOffset DeclinedAt, string DeclinedBy) : IDomainEvent;
//public record DocumentUnlocked(Guid DocumentId, DateTimeOffset DeclinedAt, string DeclinedBy) : IDomainEvent;

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
    int CurrentVersion,
    FileRevision? CurrentFileRevision,
    IReadOnlyList<FileRevision> Revisions) : AggregateBase<Document>(Id)
{
    public static Document Create(DocumentAdded added) => new(
        added.DocumentId,
        added.Alias,
        added.CreatedAt,
        added.CreatedBy,
        CurrentVersion: 0,
        CurrentFileRevision: null,
        OngoingApprovalProcess: false,
        Revisions: []);
    
    protected override Document ApplyEventInternal(IDomainEvent @event)=> @event switch
    {
        CandidateFileRevisionUploaded uploaded => Apply(uploaded),
        CandidateFileRevisionApproved approved => Apply(approved),
        CandidateFileRevisionDeclined declined => Apply(declined),
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
}