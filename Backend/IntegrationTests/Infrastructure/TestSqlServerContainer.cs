using DotNet.Testcontainers.Builders;
using Testcontainers.MsSql;

namespace IntegrationTests.Infrastructure;

public class TestSqlServerContainer : IAsyncLifetime
{
    public MsSqlContainer Container { get; }

    public string ConnectionString => Container.GetConnectionString();

    public TestSqlServerContainer()
    {
        Container = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Password@123")
            .WithEnvironment("ACCEPT_EULA", "Y")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .WithAutoRemove(true)
            .WithCleanUp(true)
            .Build();
    }

    public Task InitializeAsync() => Container.StartAsync();

    public Task DisposeAsync() => Container.DisposeAsync().AsTask();
}
