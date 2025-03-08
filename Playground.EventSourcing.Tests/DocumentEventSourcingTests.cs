using Marten;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Projections;
using Playground.EventSourcing.Tests.Common;
using Shouldly;

namespace Playground.EventSourcing.Tests;

public class ProductEventSourcingTests(PostgresTestContainerFixture postgresFixture) : TestsBase(postgresFixture)
{
    [Fact]
    public async Task GivenSomeProductEvents_WhenTheAggregateIsBuilt_ThenItShouldBeCorrect()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();

        session.Events.StartStream<Product>(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionApproved(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionApproved(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionApproved(productId)
        );
        await session.SaveChangesAsync();

        // Act
        var productAggregate = await session.Events.AggregateStreamAsync<Product>(productId);

        // Assert
        await Verify(productAggregate);
    }

    [Fact]
    public async Task GivenANewCandidateFileRevision_WhenProjectionIsAsked_ThenItShouldShowTheCandidate()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();

        session.Events.StartStream<Product>(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionDeclined(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionApproved(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId)
        );
        await session.SaveChangesAsync();

        // Act
        var candidateFileRevision = await session.LoadAsync<CandidateFileRevisionState>(productId);

        // Assert
        await Verify(candidateFileRevision);
    }

    [Fact]
    public async Task GivenADeclinedFileRevision_WhenProjectionIsAsked_ThenItShouldReturnNull()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();

        session.Events.StartStream<Product>(productId,
            FakeEvent.ProductAdded(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionDeclined(productId)
        );
        await session.SaveChangesAsync();

        // Act
        var candidateFileRevision = await session.LoadAsync<CandidateFileRevisionState>(productId);

        // Assert
        candidateFileRevision.ShouldBeNull();
    }

    [Fact]
    public async Task GivenAMultipleEventsOverProducts_WhenQueryingHistory_ThenItShouldReturnAllEvents()
    {
        // Arrange
        await using var session = NewSession();

        var productId1 = Guid.NewGuid();
        session.Events.StartStream(productId1,
            FakeEvent.ProductAdded(productId1),
            FakeEvent.CandidateFileRevisionUploaded(productId1),
            FakeEvent.CandidateFileRevisionApproved(productId1)
        );

        var productId2 = Guid.NewGuid();
        session.Events.Append(productId2,
            FakeEvent.ProductAdded(productId2),
            FakeEvent.CandidateFileRevisionUploaded(productId2),
            FakeEvent.CandidateFileRevisionDeclined(productId2)
        );

        await session.SaveChangesAsync();

        // Act
        var history = await session.Query<CandidateFileRevisionHistoryEntry>()
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
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionApproved(productId)
        );
        await session.SaveChangesAsync();

        // Load saved Aggregate
        var product = await session.Events.AggregateStreamAsync<Product>(productId);
        product.ShouldNotBeNull();

        // Add uncommited events via the aggregate
        product.ApplyEvents(
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionDeclined(productId),
            FakeEvent.CandidateFileRevisionUploaded(productId),
            FakeEvent.CandidateFileRevisionApproved(productId));

        // Act
        session.Events.AppendUncommitedEventsAndClear(product);
        await session.SaveChangesAsync();

        // Assert
        var reloadedProduct = await session.Events.AggregateStreamAsync<Product>(productId);
        await Verify(reloadedProduct);
    }
}