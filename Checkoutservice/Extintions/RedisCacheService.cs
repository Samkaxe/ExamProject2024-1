using StackExchange.Redis;
using Polly.Retry;
using Polly.CircuitBreaker;
using System.Diagnostics;
using Checkoutservice.Extintions;
using Microsoft.Extensions.Options;

public class RedisCacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreakerPolicy;
    private static readonly ActivitySource ActivitySource = new ActivitySource("CheckoutService");
    private readonly FeatureToggleSettings _featureToggleSettings;

    public RedisCacheService(string connectionString, ILogger<RedisCacheService> logger, AsyncRetryPolicy retryPolicy, AsyncCircuitBreakerPolicy circuitBreakerPolicy, FeatureToggleSettings featureToggleSettings)
    {
        _logger = logger;
        _retryPolicy = retryPolicy;
        _circuitBreakerPolicy = circuitBreakerPolicy;
        _featureToggleSettings = featureToggleSettings;
        _db = ConnectToRedis(connectionString).GetAwaiter().GetResult();
    }

    private async Task<IDatabase> ConnectToRedis(string connectionString)
    {
        using var activity = ActivitySource.StartActivity("ConnectToRedis");
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            return await _circuitBreakerPolicy.ExecuteAsync(async () =>
            {
                try
                {
                    var connection = await ConnectionMultiplexer.ConnectAsync(connectionString);
                    _logger.LogInformation("Connected to Redis successfully.");
                    return connection.GetDatabase();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to connect to Redis.");
                    throw;
                }
            });
        });
    }

    public void Set(string key, string value, TimeSpan expiry)
    {
        using var activity = ActivitySource.StartActivity("Set");
        var stopwatch = Stopwatch.StartNew();
        var process = Process.GetCurrentProcess();
    
        try
        {
            var cpuUsageStart = process.TotalProcessorTime;
            var memoryUsageStart = process.WorkingSet64;

            _db.StringSet(key, value, expiry);
        
            var cpuUsageEnd = process.TotalProcessorTime;
            var memoryUsageEnd = process.WorkingSet64;

            stopwatch.Stop();

            var cpuUsage = cpuUsageEnd - cpuUsageStart;
            var memoryUsage = memoryUsageEnd - memoryUsageStart;
        
            _logger.LogInformation("Set {Key} to {Value} with expiry {Expiry}", key, value, expiry);
            _logger.LogInformation("CPU Usage: {CpuUsage}ms, Memory Usage: {MemoryUsage} bytes, Duration: {Duration}ms", 
                cpuUsage.TotalMilliseconds, memoryUsage, stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Error setting key {Key} in Redis. Duration: {Duration}ms", key, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    public string Get(string key)
    {
        using var activity = ActivitySource.StartActivity("Get");
        try
        {
            var value = _db.StringGet(key);
            _logger.LogInformation("Retrieved {Key}: {Value}", key, value);
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving key {Key} from Redis.", key);
            throw;
        }
    }

    public void SetCheckoutSession(string userId, string orderId)
    {
        if (_featureToggleSettings.EnableCheckoutFeature)
        {
            _logger.LogInformation("EnableCheckoutFeature: true");
            _logger.LogInformation("Checkout feature is disabled.");
            return;
        }

        using var activity = ActivitySource.StartActivity("SetCheckoutSession");
        var key = $"checkout:{userId}";
        try
        {
            _db.HashSet(key, new HashEntry[] { new HashEntry("orderId", orderId) });
            _db.KeyExpire(key, TimeSpan.FromDays(7));
            _logger.LogInformation("Checkout session created for User: {UserId} with Order: {OrderId}", userId, orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting checkout session for User: {UserId}", userId);
            throw;
        }
    }

    public string GetCheckoutOrderId(string userId)
    {
        using var activity = ActivitySource.StartActivity("GetCheckoutOrderId");
        var key = $"checkout:{userId}";
        try
        {
            var value = _db.HashGet(key, "orderId");
            if (!value.IsNull)
            {
                _logger.LogInformation("Retrieved Order ID: {OrderId} for User: {UserId}", value, userId);
                return value.ToString();
            }
            else
            {
                _logger.LogWarning("No checkout session found for User: {UserId}", userId);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving checkout session for User: {UserId}", userId);
            throw;
        }
    }
    
    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var activity = ActivitySource.StartActivity("RedisHealthCheck");

            // Check if Redis connection is connected and active
            if (_db.Multiplexer.IsConnected && _db.Multiplexer.GetServer(_db.Multiplexer.GetEndPoints().First()).IsConnected)
            {
                _logger.LogInformation("Redis is healthy.");
                return true;
            }
            else
            {
                _logger.LogWarning("Redis connection is not active.");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis health check failed.");
            return false;
        }
    }
}
