using Auth.Domain.BusinessEntities;

namespace Auth.Infrastructure.Interfaces;

public interface ICredentialRepository
{
    public Task<Credentials> RegisterCredentials(Credentials credentials);

    public Task<Credentials> GetCredentialsByEmailAsync(string email);
    
}