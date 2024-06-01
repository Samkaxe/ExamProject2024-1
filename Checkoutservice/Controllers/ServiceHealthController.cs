using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Checkoutservice.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceHealthController : ControllerBase
{
    private readonly RedisCacheService _redisCacheService;
    private static readonly ActivitySource ActivitySource = new ActivitySource("CheckoutService");
    
    public ServiceHealthController(RedisCacheService redisCacheService)
    {
        _redisCacheService = redisCacheService;
    }
    
    [HttpGet("health")]
    public async Task<IActionResult> GetHealthAsync()
    {
        using var activity = ActivitySource.StartActivity("HealthCheck", ActivityKind.Server);
        var isHealthy = await _redisCacheService.IsHealthyAsync();

        if (isHealthy)
        {
            activity?.SetStatus(ActivityStatusCode.Ok);
            return Ok(new { Status = "Healthy" });
        }
        else
        {
            activity?.SetStatus(ActivityStatusCode.Error);
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Status = "Unhealthy" });
        }
    }
}