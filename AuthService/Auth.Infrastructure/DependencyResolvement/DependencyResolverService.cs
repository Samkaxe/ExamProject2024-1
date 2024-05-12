using Auth.Infrastructure.Database;
using Auth.Infrastructure.Interfaces;
using Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Infrastructure.DependencyResolvement;

public static class DependencyResolverService
{
    public static void RegisterInfrastructureLayer(IServiceCollection services)
    {
        services.AddScoped<ICredentialRepository, CredentialRepository>();
        services.AddDbContext<DatabaseContext>(options =>
            options.UseSqlite("Data Source=auth.db"));
    }
}