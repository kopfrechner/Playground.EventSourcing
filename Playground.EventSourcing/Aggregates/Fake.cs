using Bogus;

namespace Playground.EventSourcing.Aggregates;

public static partial class Fake
{
    public static DocumentAdded DocumentAdded(Guid documentId)
    {
        return new Faker<DocumentAdded>().CustomInstantiator(f =>
                new DocumentAdded(
                    documentId,
                    f.System.FileName(f.System.CommonFileExt()),
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }

    public static FileRevisionApproved FileRevisionApproved(Guid documentId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<FileRevisionApproved>().CustomInstantiator(f =>
                new FileRevisionApproved(
                    documentId,
                    f.System.FileName(fileExt),
                    $"{Guid.NewGuid()}.{fileExt}",
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
}