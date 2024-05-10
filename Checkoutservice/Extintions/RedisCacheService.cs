using StackExchange.Redis;

namespace Checkoutservice.Extintions;

public class RedisCacheService
{
    private readonly IDatabase _db;
    private readonly ILogger<RedisCacheService> _logger;

    public RedisCacheService(string connectionString, ILogger<RedisCacheService> logger)
    {
        _logger = logger;
        try
        {
            var connection = ConnectionMultiplexer.Connect(connectionString);
            _db = connection.GetDatabase();
            _logger.LogInformation("Connected to Redis successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect to Redis.");
            throw;
        }
    }
    
    public void Set(string key, string value, TimeSpan expiry)
    {
        try
        {
            _db.StringSet(key, value, expiry);
            _logger.LogInformation("Set {Key} to {Value} with expiry {Expiry}", key, value, expiry);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting key {Key} in Redis.", key);
            throw;
        }
    }

    // Example of logging within a method
    public string Get(string key)
    {
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
        var key = $"checkout:{userId}";
        _db.HashSet(key, new HashEntry[] { new HashEntry("orderId", orderId) });
        _db.KeyExpire(key, TimeSpan.FromDays(7)); 
        _logger.LogInformation("Checkout session created for User: {UserId} with Order: {OrderId}", userId, orderId);
    }

    public string GetCheckoutOrderId(string userId)
    {
        var key = $"checkout:{userId}";
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
}