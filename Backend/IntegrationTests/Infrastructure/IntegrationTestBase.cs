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

using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using API.Data;


namespace IntegrationTests.Infrastructure;

public abstract class IntegrationTestBase : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly CustomWebApplicationFactory Factory;
    protected readonly HttpClient Client;
    protected readonly AppDbContext DbContext;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Factory = factory;
        Client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false 
        }
        );

        // Create a scope to get DbContext
        var scope = factory.Services.CreateScope();
        DbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Ensure clean database for each test
        CleanDatabase();
    }

    private void CleanDatabase()
    {
        // Clean all data but keep schema
        var entityTypes = DbContext.Model.GetEntityTypes();
        foreach (var entityType in entityTypes)
        {
            var tableName = entityType.GetTableName();
            if (tableName != null)
            {
                DbContext.Database.ExecuteSqlRaw($"DELETE FROM [{tableName}]");
            }
        }
        DbContext.SaveChanges();
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
