using AutoMapper;

namespace InventoryService.Extintions;

public class HealthCheckService
{
    private readonly MongoService _mongoService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<HealthCheckService> _logger;

    public HealthCheckService(MongoService mongoService, IServiceProvider serviceProvider, ILogger<HealthCheckService> logger)
    {
        _mongoService = mongoService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task<bool> IsHealthyAsync()
    {
        bool isMongoHealthy = _mongoService.IsConnectionEstablished();
        bool isMapperHealthy = CheckMapperConfiguration();

        if (!isMongoHealthy)
        {
            _logger.LogWarning("MongoDB connection is unhealthy.");
        }
        if (!isMapperHealthy)
        {
            _logger.LogWarning("Mapper configuration is invalid.");
        }

        return isMongoHealthy && isMapperHealthy;
    }

    private bool CheckMapperConfiguration()
    {
        try
        {
            _serviceProvider.GetRequiredService<IMapper>();
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Mapper configuration validation failed.");
            return false;
        }
    }

}