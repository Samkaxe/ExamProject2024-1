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
    private readonly IEncryptionService _encryptionService;

    public AuthService(ICredentialRepository credentialRepository, IEncryptionService encryptionService)
    {
        _credentialRepository = credentialRepository;
        _encryptionService = encryptionService;
    }

    public async Task<OperationResult<BearerToken>> Register(RegisterModel registerModel)
    {
        _encryptionService.CreatePasswordHash(registerModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

        Credentials credentials = new Credentials()
        {
            Email = registerModel.Email,
            PasswordHash = passwordHash,
            Salt = passwordSalt
        };

        var createdCredentials = _credentialRepository.RegisterCredentials(credentials);

        return OperationResult<BearerToken>.CreateSuccessResult(new BearerToken() { Email = passwordHash });
    }

    public Task<OperationResult<BearerToken>> Login(LoginModel loginModel)
    {
        throw new NotImplementedException();
    }
}