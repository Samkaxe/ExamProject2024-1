using Auth.Infrastructure.Database;
using Auth.Infrastructure.Interfaces;
using Auth.Infrastructure.Migrations;
using Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.DependencyResolvement;

public static class DependencyResolverService
{
    public static void RegisterInfrastructureLayer(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        
        // Add the DatabaseInitializer
        services.AddScoped<DatabaseInitializer>();

        // Create a scope to resolve the initializer and ensure the database is created
        var serviceProvider = services.BuildServiceProvider();
        var initializer = serviceProvider.GetRequiredService<DatabaseInitializer>();
        initializer.EnsureDatabaseCreated().Wait(); // Ensure creation synchronously
    }
}