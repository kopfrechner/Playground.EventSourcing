using Bogus;
using Marten;

namespace Playground.EventSourcing.Tests.Common;

public abstract class TestsBase(PostgresTestContainerFixture postgresFixture) : IClassFixture<PostgresTestContainerFixture>, IAsyncLifetime
{
    private DocumentStore _store = null!;

    protected IDocumentSession NewSession() => _store.LightweightSession();

    public async Task InitializeAsync()
    {
        var connectionString = postgresFixture.Container.GetConnectionString();
        _store = EventStoreSetup.SetupDocumentStore(connectionString);
        await _store.Advanced.Clean.CompletelyRemoveAllAsync();
        await _store.Storage.ApplyAllConfiguredChangesToDatabaseAsync();
        
        Randomizer.Seed = new Random(420);
    }

    public async Task DisposeAsync()
    {
        await _store.DisposeAsync();
    }
}