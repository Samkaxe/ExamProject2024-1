using Npgsql;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace Auth.API.Configuration;

public static class ConfigurationService
{
    public static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenTelemetry()
            .WithTracing(providerBuilder => providerBuilder
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("Auth-Microservice"))
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddNpgsql()
                .AddJaegerExporter(options =>
                {
                    options.AgentHost = "jaeger";
                    options.AgentPort = 6831;
                })
            );
    }
}