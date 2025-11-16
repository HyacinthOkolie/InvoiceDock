// using System.Net.Http;
// using Xunit;

// namespace IntegrationTests.Infrastructure
// {
//     public abstract class IntegrationTestBase : IAsyncLifetime
//     {
//         protected TestSqlServerContainer SqlContainer;
//         protected CustomWebApplicationFactory Factory;
//         protected HttpClient Client;

//         public async Task InitializeAsync()
//         {
//             SqlContainer = new TestSqlServerContainer();
//             await SqlContainer.InitializeAsync();

//             // Factory = new CustomWebApplicationFactory(SqlContainer.ConnectionString);

//             // Client = Factory.CreateClient();
//         }

//         public async Task DisposeAsync()
//         {
//             Client?.Dispose();
//             Factory?.Dispose();
//             await SqlContainer.DisposeAsync();
//         }
//     }
// }

using System.Net.Http;
using API.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>,
        IAsyncLifetime
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly AppDbContext DbContext;
    protected readonly IServiceScope Scope;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false,
                BaseAddress = new Uri("https://localhost:5001"), // Make sure this matches your API URL
            }
        );

        // Create a scope to get DbContext
        Scope = factory.Services.CreateScope();
        DbContext = Scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }

    public async Task InitializeAsync()
    {
        // Reset database before each test
        await CleanDatabaseAsync();
    }

    public async Task DisposeAsync()
    {
        // Clean up after each test
        Scope?.Dispose();
    }

    private async Task CleanDatabaseAsync()
    {
        // Clean all data but keep schema
        var tableNames = DbContext
            .Model.GetEntityTypes()
            .Select(t => t.GetTableName())
            .Where(name => name != null)
            .ToList();

        foreach (var tableName in tableNames)
        {
            await DbContext.Database.ExecuteSqlAsync($"DELETE FROM [{tableName}]");
        }
        await DbContext.SaveChangesAsync();
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
