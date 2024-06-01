using Auth.Application.Interfaces;
using Auth.Infrastructure.Interfaces;

namespace Auth.Application.Services;

public class HealthCheckService: IHealthCheckService
{
    private readonly IDatabaseHealthCheck _databaseHealthCheck;

    public HealthCheckService(IDatabaseHealthCheck databaseHealthCheck)
    {
        _databaseHealthCheck = databaseHealthCheck;
    }

    public Task<bool> IsHealthyAsync()
    {
        return _databaseHealthCheck.IsHealthyAsync();
    }
}