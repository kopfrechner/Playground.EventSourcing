using Marten;

namespace Playground.EventSourcing.Tests;

public abstract class TestsBase : IDisposable
{
    private readonly DocumentStore _store;

    protected TestsBase()
    {
        // Configure Marten
        var connectionString = EventStoreSetup.ConnectionStringOrThrow();
        _store = EventStoreSetup.SetupDocumentStore(connectionString);
    }
    
    protected IDocumentSession Session => _store.LightweightSession();

    public void Dispose()
    {
        _store.Dispose();
    }
}