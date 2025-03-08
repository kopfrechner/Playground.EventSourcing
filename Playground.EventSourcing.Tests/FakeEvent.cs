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

    public static CandidateFileRevisionApproved CandidateFileRevisionApproved(Guid productId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<CandidateFileRevisionApproved>().CustomInstantiator(f =>
                new CandidateFileRevisionApproved(
                    productId,
                    f.System.FileName(fileExt),
                    $"{f.Random.Guid()}.{fileExt}",
                    f.Commerce.ProductDescription(),
                    f.Random.Words(20),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateFileRevisionUploaded CandidateFileRevisionUploaded(Guid productId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<CandidateFileRevisionUploaded>().CustomInstantiator(f =>
                new CandidateFileRevisionUploaded(
                    productId,
                    f.System.FileName(fileExt),
                    $"{f.Random.Guid()}.{fileExt}",
                    f.Commerce.ProductDescription(),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateFileRevisionDeclined CandidateFileRevisionDeclined(Guid productId)
    {
        return new Faker<CandidateFileRevisionDeclined>().CustomInstantiator(f =>
                new CandidateFileRevisionDeclined(
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