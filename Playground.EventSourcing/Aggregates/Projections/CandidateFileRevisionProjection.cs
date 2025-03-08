using Marten.Events.Aggregation;

namespace Playground.EventSourcing.Aggregates.Projections;

public record CandidateProductRevisionState(
    Guid Id,
    string FileName,
    string InternalUniqueFileName,
    DateTimeOffset UploadedAt,
    string UploadedBy);

public class CandidateProductRevisionProjection : SingleStreamProjection<CandidateProductRevisionState>
{
    public static CandidateProductRevisionState Create(CandidateProductRevisionUploaded @event)
    {
        return new CandidateProductRevisionState(
            @event.ProductId,
            @event.FileName,
            @event.InternalUniqueFileName,
            @event.UploadedAt,
            @event.UploadedBy);
    }

    public CandidateProductRevisionState Apply(CandidateProductRevisionState state, CandidateProductRevisionUploaded @event)
    {
        return Create(@event);
    }

    public CandidateProductRevisionState Apply(CandidateProductRevisionState state, CandidateProductRevisionApproved _) => null;

    public CandidateProductRevisionState Apply(CandidateProductRevisionState state, CandidateProductRevisionDeclined _) => null;
}