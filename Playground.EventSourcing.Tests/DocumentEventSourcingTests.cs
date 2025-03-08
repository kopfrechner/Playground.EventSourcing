using Bogus;
using Marten;
using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Aggregates.Projections;
using Shouldly;

namespace Playground.EventSourcing.Tests;

public class DocumentEventSourcingTests : TestsBase
{
    [Fact]
    public async Task GivenSomeDocumentEvents_WhenTheAggregateIsBuilt_ThenItShouldBeCorrect()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();

        session.Events.StartStream<Document>(documentId,
            FakeEvent.DocumentAdded(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionApproved(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionApproved(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionApproved(documentId)
        );
        await session.SaveChangesAsync();

        // Act
        var documentAggregate = await session.Events.AggregateStreamAsync<Document>(documentId);

        // Assert
        await Verify(documentAggregate);
    }

    [Fact]
    public async Task GivenANewCandidateFileRevision_WhenProjectionIsAsked_ThenItShouldShowTheCandidate()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();

        session.Events.StartStream<Document>(documentId,
            FakeEvent.DocumentAdded(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionDeclined(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionApproved(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId)
        );
        await session.SaveChangesAsync();

        // Act
        var candidateFileRevision = await session.LoadAsync<CandidateFileRevisionState>(documentId);

        // Assert
        await Verify(candidateFileRevision);
    }

    [Fact]
    public async Task GivenADeclinedFileRevision_WhenProjectionIsAsked_ThenItShouldReturnNull()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();
        
        session.Events.StartStream<Document>(documentId,
            FakeEvent.DocumentAdded(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionDeclined(documentId)
        );
        await session.SaveChangesAsync();

        // Act
        var candidateFileRevision = await session.LoadAsync<CandidateFileRevisionState>(documentId);

        // Assert
        candidateFileRevision.ShouldBeNull();
    }

    [Fact]
    public async Task GivenAMultipleEventsOverDocuments_WhenQueryingHistory_ThenItShouldReturnAllEvents()
    {
        // Arrange
        await using var session = NewSession();

        var documentId1 = Guid.NewGuid();
        session.Events.StartStream(documentId1,
            FakeEvent.DocumentAdded(documentId1),
            FakeEvent.CandidateFileRevisionUploaded(documentId1),
            FakeEvent.CandidateFileRevisionApproved(documentId1)
        );

        var documentId2 = Guid.NewGuid();
        session.Events.Append(documentId2,
            FakeEvent.DocumentAdded(documentId2),
            FakeEvent.CandidateFileRevisionUploaded(documentId2),
            FakeEvent.CandidateFileRevisionDeclined(documentId2)
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
    public async Task GivenADocumentsAggregate_WhenUncommitedEventsAreApplied_ThenOnlyUncommitedEventsShouldBeSaved()
    {
        // Arrange
        await using var session = NewSession();
        
        // Prepare some events
        var documentId = Guid.NewGuid();
        session.Events.Append(documentId,
            FakeEvent.DocumentAdded(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionApproved(documentId)
        );
        await session.SaveChangesAsync();
        
        // Load saved Aggregate
        var document = await session.Events.AggregateStreamAsync<Document>(documentId);
        document.ShouldNotBeNull();
        
        // Add uncommited events via the aggregate
        document.ApplyEvents(
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionDeclined(documentId),
            FakeEvent.CandidateFileRevisionUploaded(documentId),
            FakeEvent.CandidateFileRevisionApproved(documentId));
        
        // Act
        session.Events.AppendUncommitedEventsAndClear(document);
        await session.SaveChangesAsync();
        
        // Assert
        var reloadedDocument = await session.Events.AggregateStreamAsync<Document>(documentId);
        await Verify(reloadedDocument);
    }
}