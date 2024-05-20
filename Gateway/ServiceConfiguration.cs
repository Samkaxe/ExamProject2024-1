namespace Gateway;

public static class ServiceConfiguration
{
    public static void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
    {
        builder.Configuration
            .SetBasePath(builder.Environment.ContentRootPath)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("yarp.json", optional: false, reloadOnChange: true)
            .AddEnvironmentVariables();

        services.AddReverseProxy()
            .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));
    }
}