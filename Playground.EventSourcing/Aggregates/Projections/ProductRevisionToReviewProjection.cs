using Marten.Events.Aggregation;

namespace Playground.EventSourcing.Aggregates.Projections;

public record ProductRevisionToReviewState(
    Guid Id,
    string Description,
    string InternalDescription,
    DateTimeOffset UploadedAt,
    string UploadedBy);

public class ProductRevisionToReviewProjection : SingleStreamProjection<ProductRevisionToReviewState>
{
    public static ProductRevisionToReviewState Create(ProductRevisionUploaded @event)
    {
        return new ProductRevisionToReviewState(
            @event.ProductId,
            @event.Description,
            @event.InternalDescription,
            @event.UploadedAt,
            @event.UploadedBy);
    }

    public ProductRevisionToReviewState Apply(ProductRevisionToReviewState state, ProductRevisionUploaded @event)
    {
        return Create(@event);
    }

    public ProductRevisionToReviewState Apply(ProductRevisionToReviewState state, ProductRevisionApproved _) => null;

    public ProductRevisionToReviewState Apply(ProductRevisionToReviewState state, ProductRevisionDeclined _) => null;
}