
using API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>,
        IAsyncLifetime
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

    private async Task CleanDatabaseAsync()
    {
        try
        {
            Logger.LogInformation("Cleaning database tables...");

            // Use hardcoded table names to avoid parameterization issues
            var tables = new[] { "InvoiceItems", "Invoices", "Clients", "Users" };

            foreach (var tableName in tables)
            {
                await CleanTableIfExistsAsync(tableName);
            }

            await DbContext.SaveChangesAsync();
            Logger.LogInformation("Database cleaned successfully");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Failed to clean database");
            // Don't throw - allow test to run with existing data
        }
    }

    private async Task CleanTableIfExistsAsync(string tableName)
    {
        try
        {
            // Use FormattableString to avoid parameterization
            await DbContext.Database.ExecuteSqlRawAsync(
                $"IF OBJECT_ID('{tableName}', 'U') IS NOT NULL DELETE FROM [{tableName}]"
            );
            Logger.LogInformation("Cleaned table: {TableName}", tableName);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Failed to clean table {TableName}", tableName);
            // Continue with other tables
        }
    }

    protected async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return System.Text.Json.JsonSerializer.Deserialize<T>(
            content,
            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
        );
    }
}
