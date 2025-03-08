using Bogus;
using Playground.EventSourcing.Aggregates;
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
        
        Randomizer.Seed = new Random(420);
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
        
        Randomizer.Seed = new Random(420);
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
        
        Randomizer.Seed = new Random(420);
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
}