using Bogus;
using Playground.EventSourcing.Aggregates;

namespace Playground.EventSourcing.Tests;

public static class FakeEvent
{
    public static ProductAdded ProductAdded(Guid productId)
    {
        return new Faker<ProductAdded>().CustomInstantiator(f =>
                new ProductAdded(
                    productId,
                    f.Commerce.ProductName(),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }

    public static CandidateProductRevisionApproved CandidateProductRevisionApproved(Guid productId)
    {
        return new Faker<CandidateProductRevisionApproved>().CustomInstantiator(f =>
                new CandidateProductRevisionApproved(
                    productId,
                    f.Commerce.ProductDescription(),
                    f.Commerce.ProductDescription(),
                    f.Random.Words(20),
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateProductRevisionUploaded CandidateProductRevisionUploaded(Guid productId)
    {
        return new Faker<CandidateProductRevisionUploaded>().CustomInstantiator(f =>
                new CandidateProductRevisionUploaded(
                    productId,
                    f.Commerce.ProductDescription(),
                    f.Commerce.ProductDescription(),
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateProductRevisionDeclined CandidateProductRevisionDeclined(Guid productId)
    {
        return new Faker<CandidateProductRevisionDeclined>().CustomInstantiator(f =>
                new CandidateProductRevisionDeclined(
                    productId,
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static ProductLocked ProductLocked(Guid productId)
    {
        return new Faker<ProductLocked>().CustomInstantiator(f =>
                new ProductLocked(
                    productId,
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static ProductUnlocked ProductUnlocked(Guid productId)
    {
        return new Faker<ProductUnlocked>().CustomInstantiator(f =>
                new ProductUnlocked(
                    productId,
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
}