namespace Auth.Infrastructure.Interfaces;

public interface IDatabaseHealthCheck
{
    Task<bool> IsHealthyAsync();
}
