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
        Credentials credentials = new Credentials()
        {
            UserId = Guid.NewGuid(),
            Email = registerModel.Email,
            PasswordHash = "'CACA'",
            Salt = "salt"
        };
        Credentials created = await _credentialRepository.RegisterCredentials(credentials);
        return OperationResult<BearerToken>.CreateSuccessResult(new BearerToken() { Email = created.Email });
    }

    public Task<OperationResult<BearerToken>> Login(LoginModel loginModel)
    {
        throw new NotImplementedException();
    }
}