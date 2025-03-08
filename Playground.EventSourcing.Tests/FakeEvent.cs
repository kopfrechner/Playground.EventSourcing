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
                    f.System.FileName(f.System.CommonFileExt()),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }

    public static CandidateProductRevisionApproved CandidateProductRevisionApproved(Guid productId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<CandidateProductRevisionApproved>().CustomInstantiator(f =>
                new CandidateProductRevisionApproved(
                    productId,
                    f.System.FileName(fileExt),
                    $"{f.Random.Guid()}.{fileExt}",
                    f.Commerce.ProductDescription(),
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateProductRevisionUploaded CandidateProductRevisionUploaded(Guid productId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<CandidateProductRevisionUploaded>().CustomInstantiator(f =>
                new CandidateProductRevisionUploaded(
                    productId,
                    f.System.FileName(fileExt),
                    $"{f.Random.Guid()}.{fileExt}",
                    f.Commerce.ProductDescription(),
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