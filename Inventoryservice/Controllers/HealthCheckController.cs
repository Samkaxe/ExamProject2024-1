using InventoryService.Extintions;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("[controller]")]
public class HealthCheckController: ControllerBase
{
    private readonly HealthCheckService _healthCheckService; // inject HealthCheckService directly

    public HealthCheckController(HealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealthAsync()
    {
        bool isHealthy = await _healthCheckService.IsHealthyAsync();

        if (isHealthy)
        {
            return Ok(new { Status = "Healthy" });
        }
        else
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Status = "Unhealthy" });
        }
    }
}