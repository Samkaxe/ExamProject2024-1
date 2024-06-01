namespace Auth.Application.Interfaces;

public interface IHealthCheckService
{
    Task<bool> IsHealthyAsync();
}
