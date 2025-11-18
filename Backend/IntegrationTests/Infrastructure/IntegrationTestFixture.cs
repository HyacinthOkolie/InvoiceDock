using System.Text.Json;
using API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.Infrastructure;

public class IntegrationTestFixture : IAsyncLifetime
{
    private TestSqlServerContainer _sql = default!;
    private IServiceScope _scope = default!;
    private ILogger<IntegrationTestFixture> _logger = default!;
    private CustomWebApplicationFactory _factory = default!;

    public AppDbContext DbContext { get; private set; } = default!;
    public HttpClient Client { get; private set; } = default!;

    public async Task InitializeAsync()
    {
        // Start SQL container
        _sql = new TestSqlServerContainer();
        await _sql.InitializeAsync();

        // Create application factory
        _factory = new CustomWebApplicationFactory();
        _factory.UseTestDatabase(_sql.ConnectionString);
        Client = _factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                // HandleCookies = true,
                BaseAddress = new Uri("http://localhost:5000"),
            }
        );

        // Build the app host
        _scope = _factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await TestDatabaseInitializer.InitializeAsync(_factory.Services);
        _logger = _scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestFixture>>();

        await EnsureDatabaseAccessibleAsync();
    }

    public async Task DisposeAsync()
    {
        await _sql.DisposeAsync();
        _scope.Dispose();
        Client.Dispose();
        _factory.Dispose();
    }

    private async Task EnsureDatabaseAccessibleAsync()
    {
        _logger.LogInformation("Verifying database accessibility...");
        try
        {
            var canConnect = await DbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Cannot connect to test database");
            }
            _logger.LogInformation("Database connection verified");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Database is not accessible");
            throw;
        }
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
