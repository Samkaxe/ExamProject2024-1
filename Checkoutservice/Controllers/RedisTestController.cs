using Checkoutservice.Extintions;
using Microsoft.AspNetCore.Mvc;

namespace Checkoutservice.Controllers;

[ApiController]
[Route("[controller]")]
public class RedisTestController : ControllerBase
{
    private readonly RedisCacheService _redisCacheService;

    public RedisTestController(RedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }

    [HttpGet("set")]
    public IActionResult SetTestData()
    {
        _redisCacheService.Set("testKey", "Hello Redis", TimeSpan.FromMinutes(5));
        return Ok("Data set in Redis");
    }

    [HttpGet("get")]
    public IActionResult GetTestData()
    {
        var value = _redisCacheService.Get("testKey");
        return Ok($"Retrieved from Redis: {value}");
    }
}