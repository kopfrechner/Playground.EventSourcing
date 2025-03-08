using Bogus;
using Testcontainers.PostgreSql;

namespace Playground.EventSourcing.Tests.Common;

public class PostgresTestContainerFixture : IAsyncLifetime
{
    private static Faker _faker = new Faker();
    
    public PostgreSqlContainer Container { get; } = new PostgreSqlBuilder()
        .WithImage("postgres:latest")
        .WithDatabase(_faker.Random.String2(10))
        .WithUsername(_faker.Random.String2(10))
        .WithPassword(_faker.Random.String2(10))
        .WithAutoRemove(true)
        .Build();

    public async Task InitializeAsync()
    {
        await Container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await Container.StopAsync();
        await Container.DisposeAsync();
    }
}