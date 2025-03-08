namespace Playground.EventSourcing.Aggregates;

public record DocumentAdded(Guid DocumentId, string Alias, DateTimeOffset CreatedAt, string CreatedBy) : IEvent;

public record FileRevisionApproved(Guid DocumentId, string FileName, string InternalUniqueFileName, DateTimeOffset ApprovedAt, string ApprovedBy) : IEvent;

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
    int CurrentVersion,
    FileRevision? CurrentFileRevision,
    IReadOnlyList<FileRevision> Revisions)
{
    public static Document Create(DocumentAdded added) => new(
        added.DocumentId,
        added.Alias,
        added.CreatedAt,
        added.CreatedBy,
        0, null, []);

    public static Document Apply(FileRevisionApproved @event, Document document)
    {
        var nextVersion = document.CurrentVersion + 1;
        var newFileRevision = new FileRevision(
            @event.FileName,
            @event.InternalUniqueFileName,
            nextVersion,
            @event.ApprovedAt,
            @event.ApprovedBy);

        return document with
        {
            CurrentVersion = nextVersion,
            CurrentFileRevision = newFileRevision,
            Revisions = [.. document.Revisions, newFileRevision]
        };
    }
};