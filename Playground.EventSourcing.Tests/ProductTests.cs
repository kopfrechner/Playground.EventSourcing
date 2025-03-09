using Marten;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Projections;
using Playground.EventSourcing.Tests.Common;
using Shouldly;

namespace Playground.EventSourcing.Tests;

public class ProductTests(PostgresTestContainerFixture postgresFixture) : TestsBase(postgresFixture)
{
    [Fact]
    public async Task GivenSomeProductEvents_WhenTheAggregateIsBuilt_ThenItShouldBeCorrect()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();

        session.Events.StartStream<Product>(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionApproved(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionApproved(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionApproved(productId)
        );
        await session.SaveChangesAsync();

        // Act
        var productAggregate = await session.Events.AggregateStreamAsync<Product>(productId);

        // Assert
        await Verify(productAggregate);
    }

    [Fact]
    public async Task GivenANewCandidateProductRevision_WhenProjectionIsAsked_ThenItShouldShowTheCandidate()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();

        session.Events.StartStream<Product>(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionDeclined(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionApproved(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId)
        );
        await session.SaveChangesAsync();

        // Act
        var candidateProductRevision = await session.LoadAsync<ProductRevisionToReviewState>(productId);

        // Assert
        await Verify(candidateProductRevision);
    }

    [Fact]
    public async Task GivenADeclinedProductRevision_WhenProjectionIsAsked_ThenItShouldReturnNull()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();

        session.Events.StartStream<Product>(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionDeclined(productId)
        );
        await session.SaveChangesAsync();

        // Act
        var candidateProductRevision = await session.LoadAsync<ProductRevisionToReviewState>(productId);

        // Assert
        candidateProductRevision.ShouldBeNull();
    }

    [Fact]
    public async Task GivenAMultipleEventsOverProducts_WhenQueryingHistory_ThenItShouldReturnAllEvents()
    {
        // Arrange
        await using var session = NewSession();

        var productId1 = Guid.NewGuid();
        session.Events.StartStream(productId1,
            FakeEvent.ProductAdded(productId1),
            FakeEvent.CandidateProductRevisionUploaded(productId1),
            FakeEvent.CandidateProductRevisionApproved(productId1)
        );

        var productId2 = Guid.NewGuid();
        session.Events.Append(productId2,
            FakeEvent.ProductAdded(productId2),
            FakeEvent.CandidateProductRevisionUploaded(productId2),
            FakeEvent.CandidateProductRevisionDeclined(productId2)
        );

        await session.SaveChangesAsync();

        // Act
        var history = await session.Query<ProductRevisionHistoryEntry>()
            .OrderBy(x => x.EventTime)
            .ToListAsync();

        // Assert
        await Verify(history);
    }

    [Fact]
    public async Task GivenAProductsAggregate_WhenUncommitedEventsAreApplied_ThenOnlyUncommitedEventsShouldBeSaved()
    {
        // Arrange
        await using var session = NewSession();

        // Prepare some events
        var productId = Guid.NewGuid();
        session.Events.Append(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionApproved(productId)
        );
        await session.SaveChangesAsync();

        // Load saved Aggregate
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        product.ShouldNotBeNull();

        // Add uncommited events via the aggregate
        product.ApplyEvents(
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionDeclined(productId),
            FakeEvent.CandidateProductRevisionUploaded(productId),
            FakeEvent.CandidateProductRevisionApproved(productId));

        // Act
        session.Events.AppendAndClearUncommitedEvents(product);
        await session.SaveChangesAsync();

        // Assert
        var reloadedProduct = await session.Events.AggregateStreamAsync<Product>(productId);
        await Verify(reloadedProduct);
    }
}