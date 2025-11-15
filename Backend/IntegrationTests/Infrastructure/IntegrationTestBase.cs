using System.Net.Http;
using Xunit;

namespace IntegrationTests.Infrastructure
{
    public abstract class IntegrationTestBase : IAsyncLifetime
    {
        protected TestSqlServerContainer SqlContainer;
        protected CustomWebApplicationFactory Factory;
        protected HttpClient Client;

        public async Task InitializeAsync()
        {
            SqlContainer = new TestSqlServerContainer();
            await SqlContainer.InitializeAsync();

            // Factory = new CustomWebApplicationFactory(SqlContainer.ConnectionString);

            // Client = Factory.CreateClient();
        }

        public async Task DisposeAsync()
        {
            Client?.Dispose();
            Factory?.Dispose();
            await SqlContainer.DisposeAsync();
        }
    }
}
