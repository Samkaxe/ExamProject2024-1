using Auth.API.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.Entities;
using Auth.Domain.OperationResults;
using Auth.Domain.Security;
using Auth.Infrastructure.Interfaces;

namespace Auth.Application.Services;

public class AuthService: IAuthService
{

    private readonly ICredentialRepository _credentialRepository;

    public AuthService(ICredentialRepository credentialRepository)
    {
        _credentialRepository = credentialRepository;
    }

    public async Task<OperationResult<BearerToken>> Register(RegisterModel registerModel)
    {
        
    }

    public Task<OperationResult<BearerToken>> Login(LoginModel loginModel)
    {
        throw new NotImplementedException();
    }
}