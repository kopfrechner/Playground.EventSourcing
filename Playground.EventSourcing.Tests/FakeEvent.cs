using Bogus;
using Playground.EventSourcing.Aggregates;

namespace Playground.EventSourcing.Tests;

public static class FakeEvent
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

    public static CandidateFileRevisionApproved CandidateFileRevisionApproved(Guid documentId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<CandidateFileRevisionApproved>().CustomInstantiator(f =>
                new CandidateFileRevisionApproved(
                    documentId,
                    f.System.FileName(fileExt),
                    $"{f.Random.Guid()}.{fileExt}",
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateFileRevisionUploaded CandidateFileRevisionUploaded(Guid documentId)
    {
        var fileExt = new Faker().System.FileExt();
        return new Faker<CandidateFileRevisionUploaded>().CustomInstantiator(f =>
                new CandidateFileRevisionUploaded(
                    documentId,
                    f.System.FileName(fileExt),
                    $"{f.Random.Guid()}.{fileExt}",
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
    
    public static CandidateFileRevisionDeclined CandidateFileRevisionDeclined(Guid documentId)
    {
        return new Faker<CandidateFileRevisionDeclined>().CustomInstantiator(f =>
                new CandidateFileRevisionDeclined(
                    documentId,
                    DateTimeOffset.Now,
                    f.Name.FullName()))
            .Generate();
    }
}