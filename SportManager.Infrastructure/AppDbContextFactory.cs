using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace SportManager.Infrastructure;

public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        // Find the project root path (where the startup project is)
        // Adjust these paths based on your actual project structure
        string projectRootPath = Path.Combine(Directory.GetCurrentDirectory(), "../SportManager.API");

        // Build configuration from appsettings.json in the startup project
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .SetBasePath(projectRootPath)
            .AddJsonFile("appsettings.json")
            .Build();

        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
            npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(30),
                    errorCodesToAdd: null);
                npgsqlOptions.CommandTimeout(60);
            });

        return new AppDbContext(optionsBuilder.Options);
    }
}