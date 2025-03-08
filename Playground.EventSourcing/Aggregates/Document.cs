using Playground.EventSourcing.Aggregates.Common;

namespace Playground.EventSourcing.Aggregates;

public record DocumentAdded(Guid DocumentId, string Alias, DateTimeOffset CreatedAt, string CreatedBy) : IEvent;
public record CandidateFileRevisionUploaded(Guid DocumentId, string FileName, string InternalUniqueFileName, DateTimeOffset UploadedAt, string UploadedBy) : IEvent;
public record CandidateFileRevisionApproved(Guid DocumentId, string FileName, string InternalUniqueFileName, DateTimeOffset ApprovedAt, string ApprovedBy) : IEvent;
public record CandidateFileRevisionDeclined(Guid DocumentId, DateTimeOffset DeclinedAt, string DeclinedBy) : IEvent;

public sealed record FileRevision(
    string FileName,
    string InternalUniqueFileName,
    int Revision,
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
    IReadOnlyList<FileRevision> Revisions)
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

    public static Document Apply(CandidateFileRevisionUploaded @event, Document document)
    {
        if (document.OngoingApprovalProcess)
        {
            throw new InvalidOperationException();
        }
        
        return document with
        {
            OngoingApprovalProcess = true
        };
    }
    
    public static Document Apply(CandidateFileRevisionApproved @event, Document document)
    {
        if (!document.OngoingApprovalProcess)
        {
            throw new InvalidOperationException("To add a file revision, there must be an ongoing approval process.");
        }
        
        var nextVersion = document.CurrentVersion + 1;
        var newFileRevision = new FileRevision(
            @event.FileName,
            @event.InternalUniqueFileName,
            nextVersion,
            @event.ApprovedAt,
            @event.ApprovedBy);

        return document with
        {
            OngoingApprovalProcess = false,
            CurrentVersion = nextVersion,
            CurrentFileRevision = newFileRevision,
            Revisions = [.. document.Revisions, newFileRevision]
        };
    }
    
    public static Document Apply(CandidateFileRevisionDeclined @event, Document document)
    {
        if (!document.OngoingApprovalProcess)
        {
            throw new InvalidOperationException();
        }
        
        return document with
        {
            OngoingApprovalProcess = false
        };
    }
}