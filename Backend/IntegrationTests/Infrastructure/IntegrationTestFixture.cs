using System.Text.Json;
using API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace IntegrationTests.Infrastructure;

public class IntegrationTestFixture : IAsyncLifetime
{
    public TestSqlServerContainer Sql { get; private set; } = default!;
    public CustomWebApplicationFactory Factory { get; private set; } = default!;

    public AppDbContext DbContext { get; private set; } = default!;

    public HttpClient Client { get; private set; } = default!;

    protected IServiceScope Scope { get; private set; } = default!;


    // public HttpClient Client => Factory.CreateClient();

    public async Task InitializeAsync()
    {
        // Start SQL container
        Sql = new TestSqlServerContainer();
        await Sql.InitializeAsync();

        // Create application factory
        Factory = new CustomWebApplicationFactory();
        Factory.UseTestDatabase(Sql.ConnectionString);
        Client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                // HandleCookies = true,
                BaseAddress = new Uri("http://localhost:5000"),
            }
        );

        // Build the app host
        Scope = Factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDatabaseInitializer.InitializeAsync(Factory.Services);
    }

    public async Task DisposeAsync()
    {
        await Sql.DisposeAsync();
        Scope.Dispose();
        Client.Dispose();
        Factory.Dispose();
    }

    protected async Task<T?> DeserializeResponseAsync<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(
            content,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
}
