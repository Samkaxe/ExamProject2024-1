using Auth.Application.Interfaces;
using Auth.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Auth.Application.DependencyResolvement;

public static class DependencyResolverService
{
    public static void RegisterApplicationLayer(IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
    }
}