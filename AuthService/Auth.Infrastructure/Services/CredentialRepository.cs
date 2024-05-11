using Auth.Domain.Entities;
using Auth.Infrastructure.Interfaces;

namespace Auth.Infrastructure.Services;

public class CredentialRepository: ICredentialRepository
{
    // Dictionary to simulate data storage (replace with database later)
    private readonly Dictionary<string, Credentials> _credentialsStore = new Dictionary<string, Credentials>();

    public Task<Credentials> RegisterCredentials(Credentials credentials)
    {
        if (_credentialsStore.ContainsKey(credentials.Email))
        {
            throw new Exception("Email is already in use"); // Simulate duplicate email error
        }

        _credentialsStore.Add(credentials.Email, credentials);
        return Task.FromResult(credentials);
    }

    public Task<Credentials> GetCredentialsByEmailAsync(string email)
    {
        if (_credentialsStore.TryGetValue(email, out var credentials))
        {
            return Task.FromResult(credentials);
        }

        throw new Exception("Credentials not found for the given email"); // Simulate not found error
    }
}