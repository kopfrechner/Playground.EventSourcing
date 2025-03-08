using Playground.EventSourcing.Aggregates;
using Playground.EventSourcing.Tests.Common;
using Shouldly;

namespace Playground.EventSourcing.Tests;

public class ProductLockTests(PostgresTestContainerFixture postgresFixture) :TestsBase(postgresFixture)
{
    [Fact]
    public async Task GivenAProductsAggregate_WhenItIsLocked_ThenLockedPropertiesShouldHaveBeenApplied()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();
        var product = Product
            .Create(FakeEvent.ProductAdded(productId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(productId),
                FakeEvent.CandidateFileRevisionApproved(productId));
        
        // Act
        product = product.ApplyEvent(FakeEvent.ProductLocked(productId));
        
        // Assert
        await Verify(product);
    }
    
    [Fact]
    public async Task GivenAProductsAggregate_WhenItIsUnlocked_ThenLockedPropertiesShouldHaveBeenApplied()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();
        var product = Product
            .Create(FakeEvent.ProductAdded(productId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(productId),
                FakeEvent.CandidateFileRevisionApproved(productId), 
                FakeEvent.ProductLocked(productId)
            );
        
        // Act
        product = product.ApplyEvent(FakeEvent.ProductUnlocked(productId));
        
        // Assert
        await Verify(product);
    }
    
    [Fact]
    public async Task GivenAProductsAggregate_WhenItIsLockedTwice_ThenShouldThrowException()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();
        var product = Product
            .Create(FakeEvent.ProductAdded(productId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(productId),
                FakeEvent.CandidateFileRevisionApproved(productId), 
                FakeEvent.ProductLocked(productId)
            );
        
        // Act
        var act = () => product = product.ApplyEvent(FakeEvent.ProductLocked(productId));
        
        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }
    
    [Fact]
    public async Task GivenAProductsAggregate_WhenItIsUnlockedTwice_ThenShouldThrowException()
    {
        // Arrange
        await using var session = NewSession();
        var productId = Guid.NewGuid();
        var product = Product
            .Create(FakeEvent.ProductAdded(productId))
            .ApplyEvents(
                FakeEvent.CandidateFileRevisionUploaded(productId),
                FakeEvent.CandidateFileRevisionApproved(productId), 
                FakeEvent.ProductLocked(productId),
                FakeEvent.ProductUnlocked(productId)
            );
        
        // Act
        var act = () => product = product.ApplyEvent(FakeEvent.ProductUnlocked(productId));
        
        // Assert
        act.ShouldThrow<InvalidOperationException>();
    }
}