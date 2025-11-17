using System.Text.Json;
using API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>,
        IDisposable
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly AppDbContext DbContext;
    protected readonly IServiceScope Scope;
    protected readonly ILogger<IntegrationTestBase> Logger;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                // HandleCookies = true,
                BaseAddress = new Uri("http://localhost:5000"),
            }
        );

        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
        Logger = Scope.ServiceProvider.GetRequiredService<ILogger<IntegrationTestBase>>();
    }

    public async Task InitializeAsync()
    {
        Logger.LogInformation("Starting test initialization...");

        // Verify database is accessible before cleaning
        await EnsureDatabaseAccessibleAsync();
        // await CleanDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        Logger.LogInformation("Cleaning up test...");
        Scope?.Dispose();
        Client?.Dispose();
    }

    private async Task EnsureDatabaseAccessibleAsync()
    {
        Console.WriteLine("Verifying database accessibility...");
        try
        {
            var canConnect = await DbContext.Database.CanConnectAsync();
            if (!canConnect)
            {
                throw new InvalidOperationException("Cannot connect to test database");
            }
            Logger.LogInformation("Database connection verified");
        }
        catch (Exception ex)
        {
            Logger.LogCritical(ex, "Database is not accessible");
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

    public void Dispose() { }
}
