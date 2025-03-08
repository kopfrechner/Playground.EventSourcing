using Marten.Events.Aggregation;

namespace Playground.EventSourcing.Aggregates.Projections;

public record CandidateFileRevisionState(
    Guid Id,
    string FileName,
    string InternalUniqueFileName,
    DateTimeOffset UploadedAt,
    string UploadedBy);

public class CandidateFileRevisionProjection : SingleStreamProjection<CandidateFileRevisionState>
{
    public static CandidateFileRevisionState Create(CandidateFileRevisionUploaded @event)
    {
        return new CandidateFileRevisionState(
            @event.DocumentId,
            @event.FileName,
            @event.InternalUniqueFileName,
            @event.UploadedAt,
            @event.UploadedBy);
    }

    public CandidateFileRevisionState Apply(CandidateFileRevisionState state, CandidateFileRevisionUploaded @event)
    {
        return Create(@event);
    }

    public CandidateFileRevisionState Apply(CandidateFileRevisionState state, CandidateFileRevisionApproved _) => null;

    public CandidateFileRevisionState Apply(CandidateFileRevisionState state, CandidateFileRevisionDeclined _) => null;
}