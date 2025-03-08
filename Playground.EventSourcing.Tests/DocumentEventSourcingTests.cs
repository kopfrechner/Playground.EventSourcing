using Bogus;
using Marten.Internal.Sessions;
using Playground.EventSourcing.Aggregates;

namespace Playground.EventSourcing.Tests;

public class DocumentEventSourcingTests : TestsBase
{
    [Fact]
    public async Task Test1()
    {
        // Arrange
        await using var session = Session;
        var documentId = Guid.NewGuid();
        
        Randomizer.Seed = new Random(420);
        session.Events.StartStream<Document>(documentId,
            Fake.DocumentAdded(documentId),
            Fake.FileRevisionApproved(documentId),
            Fake.FileRevisionApproved(documentId),
            Fake.FileRevisionApproved(documentId)
        );
        await session.SaveChangesAsync();

        // Act
        var documentAggregate = await session.Events.AggregateStreamAsync<Document>(documentId);

        // Assert
        await Verify(documentAggregate);
    }
}