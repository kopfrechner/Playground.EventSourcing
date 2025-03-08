using Bogus;
using Marten;

namespace Playground.EventSourcing.Tests;

[Collection(nameof(DocumentEventSourcingTests))]
public abstract class TestsBase : IDisposable, IAsyncLifetime
{
    private readonly DocumentStore _store;

    protected TestsBase()
    {
        var connectionString = EventStoreSetup.ConnectionStringOrThrow();
        _store = EventStoreSetup.SetupDocumentStore(connectionString);
        
        Randomizer.Seed = new Random(420);
    }
    
    protected IDocumentSession NewSession() => _store.LightweightSession();

    public async Task InitializeAsync()
    {
        await _store.Advanced.Clean.CompletelyRemoveAllAsync();
        await _store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        _store.Dispose();
        return Task.CompletedTask;
    }
    
    public void Dispose()
    {
        _store.Dispose();
    }
}