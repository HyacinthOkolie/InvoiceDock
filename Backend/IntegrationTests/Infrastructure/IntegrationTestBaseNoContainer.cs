

namespace IntegrationTests.Infrastructure
{
    public class IntegrationTestBaseNoContainer : IClassFixture<CustomWebApplicationFactory>
    {
        protected readonly HttpClient Client;

        protected IntegrationTestBaseNoContainer(CustomWebApplicationFactory factory)
        {
            Client = factory.CreateClient();
        }
    }
}
