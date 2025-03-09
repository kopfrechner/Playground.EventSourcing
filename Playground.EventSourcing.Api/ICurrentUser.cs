using Bogus;

namespace Playground.EventSourcing.Api;

public interface ICurrentUser
{
    string FullName { get; }
}

public class FakeCurrentUser : ICurrentUser
{
    public string FullName => new Faker().Person.FullName;
}
