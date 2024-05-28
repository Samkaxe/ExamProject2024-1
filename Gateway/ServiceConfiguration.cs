using Yarp.ReverseProxy.Configuration;

namespace Gateway;

public static class ServiceConfiguration
    {
        public static void ConfigureServices(IServiceCollection services, WebApplicationBuilder builder)
        {
            // Build the configuration by setting the base path and adding configuration
            // files and environment variables
            var configuration = builder.Configuration
                .SetBasePath(builder.Environment.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile("yarp.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Configure YARP reverse proxy by loading routes and clusters from memory
            services.AddReverseProxy()
                .LoadFromMemory(CreateDynamicRoutes(configuration), LoadClusters(configuration));
        }

        // Method to create dynamic routes based on configuration flags
        private static IReadOnlyList<RouteConfig> CreateDynamicRoutes(IConfiguration configuration)
        {
            // Create a list to hold the routes
            var routes = new List<RouteConfig>();

            // Read the feature flags from the configuration
            var enableAuthRoutes = configuration.GetValue<bool>("FeatureFlags:EnableAuthRoutes");
            var enableProductRoutes = configuration.GetValue<bool>("FeatureFlags:EnableProductRoutes");

            // Add authentication routes if the feature flag is enabled
            if (enableAuthRoutes)
            {
                routes.Add(new RouteConfig
                {
                    ClusterId = "Authentication", // Cluster to which the route belongs
                    Match = new RouteMatch { Path = "/Auth/register" }, // Path pattern to match
                    Transforms = new[]
                    {
                        new Dictionary<string, string> { { "RequestHeadersCopy", "true" } } // Request transformations
                    }
                });

                routes.Add(new RouteConfig
                {
                    ClusterId = "Authentication",
                    Match = new RouteMatch { Path = "/Auth/login" },
                    Transforms = new[]
                    {
                        new Dictionary<string, string> { { "RequestHeadersCopy", "true" } }
                    }
                });
            }

            // Add product routes if the feature flag is enabled
            if (enableProductRoutes)
            {
                routes.Add(new RouteConfig
                {
                    ClusterId = "Inventory",
                    Match = new RouteMatch { Path = "/Product" },
                    Transforms = new[]
                    {
                        new Dictionary<string, string> { { "RequestHeadersCopy", "true" } }
                    }
                });
            }

            // Return the list of routes
            return routes;
        }

        // Method to load clusters from the configuration
        private static IReadOnlyList<ClusterConfig> LoadClusters(IConfiguration configuration)
        {
            // Create a list to hold the clusters
            var clusters = new List<ClusterConfig>();

            // Bind the cluster configuration section to the list
            configuration.GetSection("ReverseProxy:Clusters").Bind(clusters);

            // Return the list of clusters
            return clusters;
        }
    }