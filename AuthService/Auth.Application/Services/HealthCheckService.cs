using Auth.Application.Interfaces;

namespace Auth.Application.Services;

public class HealthCheckService: IHealthCheckService
{
    public Task<bool> IsHealthyAsync()
    {
        throw new NotImplementedException();
    }
}