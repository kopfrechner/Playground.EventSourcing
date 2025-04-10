using Playground.EventSourcing.Aggregates.Common;

namespace Playground.EventSourcing.Aggregates;

public record ProductAdded(Guid ProductId, string Alias, DateTimeOffset CreatedAt, string CreatedBy) : IDomainEvent;
public record ProductRevisionUploaded(Guid ProductId, string Description, string InternalDescription, string ChangeLog, DateTimeOffset UploadedAt, string UploadedBy) : IDomainEvent;
public record ProductRevisionApproved(Guid ProductId, string Description, string InternalDescription, string ChangeLog, string ApprovalComment, DateTimeOffset ApprovedAt, string ApprovedBy) : IDomainEvent;
public record ProductRevisionDeclined(Guid ProductId, string DeclinedComment, DateTimeOffset DeclinedAt, string DeclinedBy) : IDomainEvent;
public record ProductLocked(Guid ProductId, string LockReason, DateTimeOffset LockedAt, string LockedBy) : IDomainEvent;
public record ProductUnlocked(Guid ProductId, string UnlockReason, DateTimeOffset UnlockedAt, string UnlockedBy) : IDomainEvent;

public sealed record ProductRevision(
    string Description,
    string InternalDescription,
    int Revision,
    string ChangeLog,
    DateTimeOffset ApprovedAt,
    string ApprovedBy
);

public sealed record Product(
    Guid Id,
    string Name,
    DateTimeOffset CreatedAt,
    string CreatedBy,
    bool OngoingApprovalProcess,
    bool IsLocked,
    string? LockChangedReason,
    DateTimeOffset? LockedChangedAt,
    int CurrentVersion,
    ProductRevision? CurrentProductRevision,
    IReadOnlyList<ProductRevision> Revisions) : AggregateBase<Product>(Id)
{
    public static Product Create(ProductAdded added) => new(
        added.ProductId,
        added.Alias,
        added.CreatedAt,
        added.CreatedBy,
        IsLocked: false,
        LockChangedReason: null,
        LockedChangedAt: null,
        CurrentVersion: 0,
        CurrentProductRevision: null,
        OngoingApprovalProcess: false,
        Revisions: []);
    
    protected override Product ApplyEventInternal(IDomainEvent @event)=> @event switch
    {
        ProductRevisionUploaded uploaded => Apply(uploaded),
        ProductRevisionApproved approved => Apply(approved),
        ProductRevisionDeclined declined => Apply(declined),
        ProductLocked locked => Apply(locked),
        ProductUnlocked unlocked => Apply(unlocked),
        _ => throw new InvalidOperationException($"Unsupported event type: {@event.GetType().Name}")
    };
    
    private Product Apply(ProductRevisionUploaded _)
    {
        if (OngoingApprovalProcess) throw new InvalidOperationException("Already a product in Review");
        
        return this with
        {
            OngoingApprovalProcess = true
        };
    }
    
    private Product Apply(ProductRevisionApproved @event)
    {
        if (!OngoingApprovalProcess) throw new InvalidOperationException("To add a product revision, there must be an ongoing approval process.");
        
        var nextVersion = CurrentVersion + 1;
        var newProductRevision = new ProductRevision(
            @event.Description,
            @event.InternalDescription,
            nextVersion,
            @event.ChangeLog,
            @event.ApprovedAt,
            @event.ApprovedBy);

        return this with
        {
            OngoingApprovalProcess = false,
            CurrentVersion = nextVersion,
            CurrentProductRevision = newProductRevision,
            Revisions = [.. Revisions, newProductRevision]
        };
    }
    
    private Product Apply(ProductRevisionDeclined _)
    {
        if (!OngoingApprovalProcess) throw new InvalidOperationException("No product to review");
        
        return this with
        {
            OngoingApprovalProcess = false
        };
    }
    
    private Product Apply(ProductLocked locked)
    {
        if (IsLocked) throw new InvalidOperationException("Tried to lock a product that is already locked.");
        
        return this with
        {
            IsLocked = true,
            LockChangedReason = locked.LockReason,
            LockedChangedAt = locked.LockedAt
        };
    }
    
    private Product Apply(ProductUnlocked unlocked)
    {
        if (!IsLocked) throw new InvalidOperationException("Tried to unlock a product that is already unlocked.");
        
        return this with
        {
            IsLocked = false,
            LockChangedReason = unlocked.UnlockReason,
            LockedChangedAt = unlocked.UnlockedAt
        };
    }
}