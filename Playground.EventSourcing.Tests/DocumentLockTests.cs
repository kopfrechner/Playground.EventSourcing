using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Tests.Common;
using Shouldly;

namespace Playground.EventSourcing.Tests;

public class DocumentLockTests(PostgresTestContainerFixture postgresFixture) :TestsBase(postgresFixture)
{
    [Fact]
    public async Task GivenADocumentsAggregate_WhenItIsLocked_ThenLockedPropertiesShouldHaveBeenApplied()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();
        var document = Document
            .Create(FakeEvent.DocumentAdded(documentId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(documentId),
                FakeEvent.CandidateFileRevisionApproved(documentId));
        
        // Act
        document = document.ApplyEvent(FakeEvent.DocumentLocked(documentId));
        
        // Assert
        await Verify(document);
    }
    
    [Fact]
    public async Task GivenADocumentsAggregate_WhenItIsUnlocked_ThenLockedPropertiesShouldHaveBeenApplied()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();
        var document = Document
            .Create(FakeEvent.DocumentAdded(documentId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(documentId),
                FakeEvent.CandidateFileRevisionApproved(documentId), 
                FakeEvent.DocumentLocked(documentId)
            );
        
        // Act
        document = document.ApplyEvent(FakeEvent.DocumentUnlocked(documentId));
        
        // Assert
        await Verify(document);
    }
    
    [Fact]
    public async Task GivenADocumentsAggregate_WhenItIsLockedTwice_ThenShouldThrowException()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();
        var document = Document
            .Create(FakeEvent.DocumentAdded(documentId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(documentId),
                FakeEvent.CandidateFileRevisionApproved(documentId), 
                FakeEvent.DocumentLocked(documentId)
            );
        
        // Act
        var act = () => document = document.ApplyEvent(FakeEvent.DocumentLocked(documentId));
        
        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public async Task GivenADocumentsAggregate_WhenItIsUnlockedTwice_ThenShouldThrowException()
    {
        // Arrange
        await using var session = NewSession();
        var documentId = Guid.NewGuid();
        var document = Document
            .Create(FakeEvent.DocumentAdded(documentId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(documentId),
                FakeEvent.CandidateFileRevisionApproved(documentId), 
                FakeEvent.DocumentLocked(documentId),
                FakeEvent.DocumentUnlocked(documentId)
            );
        
        // Act
        var act = () => document = document.ApplyEvent(FakeEvent.DocumentUnlocked(documentId));
        
        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }
}