using Auth.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;

[ApiController]
[Route("[controller]")]
public class ServiceHealthController: ControllerBase
{

    private readonly IHealthCheckService _healthCheckService;

    public ServiceHealthController(IHealthCheckService healthCheckService)
    {
        _healthCheckService = healthCheckService;
    }

    [HttpGet("health")]
    public async Task<IActionResult> GetHealthAsync()
    {
        bool isMicroserviceHealthy = await _healthCheckService.IsHealthyAsync();

        if (isMicroserviceHealthy)
        {
            return Ok(new { Status = "Healthy" });
        }
        else
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { Status = "Unhealthy" });
        }
    }

}