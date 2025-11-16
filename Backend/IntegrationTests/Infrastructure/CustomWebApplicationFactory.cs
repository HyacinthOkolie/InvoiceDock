// using API.Data;
// using Microsoft.AspNetCore.Hosting;
// using Microsoft.AspNetCore.Mvc.Testing;
// using Microsoft.EntityFrameworkCore;
// using Microsoft.Extensions.DependencyInjection;

// namespace IntegrationTests.Infrastructure
// {
//     // Custom WebApplicationFactory to configure the test server
//     // Required for overriding config + using real DB
//     public class CustomWebApplicationFactory : WebApplicationFactory<Program>
//     {
//         private readonly string _connectionString =
//             "Server=localhost,1434;Database=InvoiceDockTest;User Id=sa;Password=Password@123;TrustServerCertificate=True";

//         // public CustomWebApplicationFactory(string connectionString)
//         // {
//         //     _connectionString = connectionString;
//         // }

//         protected override void ConfigureWebHost(IWebHostBuilder builder)
//         {
//             builder.ConfigureServices(services =>
//             {
//                 var descriptor = services.SingleOrDefault(d =>
//                     d.ServiceType == typeof(DbContextOptions<AppDbContext>)
//                 );

//                 if (descriptor != null)
//                     services.Remove(descriptor);

//                 // Add SQL Server (real DB from GitHub Pipeline)
//                 services.AddDbContext<AppDbContext>(options =>
//                     options.UseSqlServer(_connectionString)
//                 );

//                 // IMPORTANT: Build the provider and run migrations
//                 var sp = services.BuildServiceProvider();

//                 using var scope = sp.CreateScope();
//                 var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

//                 // Apply migrations so tables + seed data exist
//                 db.Database.Migrate();
//             });
//         }
//     }
// }

using API.Data;
using DotNet.Testcontainers.Builders;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.MsSql;

namespace IntegrationTests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly MsSqlContainer _dbContainer;

    public CustomWebApplicationFactory()
    {
        _dbContainer = new MsSqlBuilder()
            .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
            .WithPassword("Password@123")
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1433))
            .WithEnvironment("ACCEPT_EULA", "Y") // Accept SQL Server EULA
            // .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );

            if (descriptor != null)
                services.Remove(descriptor);

            // Add SQL Server (Testcontainer)
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(_dbContainer.GetConnectionString())
            );
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();

        // Wait longer for SQL Server to be fully ready
        // await Task.Delay(15000);

        await InitializeDatabaseAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }

    private async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope(); // Use the built Services
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<
            ILogger<CustomWebApplicationFactory>
        >();

        try
        {
            logger.LogInformation("Setting up test database...");

            // CLEAN START: Delete existing database
            await db.Database.EnsureDeletedAsync();

            // CHOOSE ONE APPROACH:

            // Option A: Use Migrations (if you have them)
            await db.Database.MigrateAsync();

            // Option B: Use EnsureCreated (if no migrations)
            // await db.Database.EnsureCreatedAsync();

            logger.LogInformation("Test database ready");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to initialize test database");
            throw;
        }
    }
}
