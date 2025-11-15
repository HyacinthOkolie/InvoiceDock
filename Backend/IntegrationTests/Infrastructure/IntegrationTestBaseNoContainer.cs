using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
