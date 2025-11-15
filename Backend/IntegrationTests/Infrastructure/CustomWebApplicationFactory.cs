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

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DotNet.Testcontainers.Builders;
using API.Data;
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
            .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(1434))
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Remove the existing DbContext registration
            var descriptor = services.SingleOrDefault(d =>
                d.ServiceType == typeof(DbContextOptions<AppDbContext>)
            );

            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Add DbContext with TestContainer connection string
            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_dbContainer.GetConnectionString());
            });

            // Build the service provider
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var scopedServices = scope.ServiceProvider;
            var db = scopedServices.GetRequiredService<AppDbContext>();
            var logger = scopedServices.GetRequiredService<ILogger<CustomWebApplicationFactory>>();

            // Ensure database is created and migrated
            try
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                // If you have migrations, run them:
                db.Database.Migrate();
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "An error occurred creating the database. Error: {Message}",
                    ex.Message
                );
            }
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        await _dbContainer.DisposeAsync();
    }
}
