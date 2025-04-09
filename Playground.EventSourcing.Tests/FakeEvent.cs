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
            .UseSeed(420)
            .Generate();
    }

    public static ProductRevisionApproved CandidateProductRevisionApproved(Guid productId)
    {
        return new Faker<ProductRevisionApproved>().CustomInstantiator(f =>
                new ProductRevisionApproved(
                    productId,
                    f.Commerce.ProductDescription(),
                    f.Commerce.ProductDescription(),
                    f.Random.Words(20),
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .UseSeed(420)
            .Generate();
    }
    
    public static ProductRevisionUploaded CandidateProductRevisionUploaded(Guid productId)
    {
        return new Faker<ProductRevisionUploaded>().CustomInstantiator(f =>
                new ProductRevisionUploaded(
                    productId,
                    f.Commerce.ProductDescription(),
                    f.Commerce.ProductDescription(),
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .UseSeed(420)
            .Generate();
    }
    
    public static ProductRevisionDeclined CandidateProductRevisionDeclined(Guid productId)
    {
        return new Faker<ProductRevisionDeclined>().CustomInstantiator(f =>
                new ProductRevisionDeclined(
                    productId,
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .UseSeed(420)
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
            .UseSeed(420)
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
            .UseSeed(420)
            .Generate();
    }
}