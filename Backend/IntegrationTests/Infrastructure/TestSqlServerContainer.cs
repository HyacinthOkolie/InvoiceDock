using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;

namespace IntegrationTests.Infrastructure
{
    public class TestSqlServerContainer : IAsyncLifetime
    {
        private readonly IContainer _container;

        public string ConnectionString =>
            $"Server=localhost,{_container.GetMappedPublicPort(1433)};User Id=sa;Password=Password@123;TrustServerCertificate=True";

        public TestSqlServerContainer()
        {
            _container = new ContainerBuilder()
                .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
                .WithEnvironment("ACCEPT_EULA", "Y")
                .WithEnvironment("SA_PASSWORD", "Password@123")
                .WithExposedPort(1433)
                .WithPortBinding(1433, true) // map to random host port
                .WithWaitStrategy(
                    Wait.ForUnixContainer()
                        .UntilMessageIsLogged("SQL Server is now ready for client connections")
                )
                .Build();
        }

        public Task InitializeAsync() => _container.StartAsync();

        public Task DisposeAsync() => _container.StopAsync();
    }
}
