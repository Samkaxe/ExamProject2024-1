using System.Diagnostics;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.BusinessEntities;
using Auth.Domain.OperationResults;
using Auth.Infrastructure.Interfaces;

namespace Auth.Application.Services;

public class AuthService: IAuthService
{
    private readonly ICredentialRepository _credentialRepository;
    private readonly IEncryptionService _encryptionService;
    private readonly ITokenService _tokenService;
    private readonly ActivitySource _activitySource;

    public AuthService(ICredentialRepository credentialRepository, IEncryptionService encryptionService,
        ITokenService tokenService, ActivitySource activitySource)
    {
        _credentialRepository = credentialRepository;
        _encryptionService = encryptionService;
        _tokenService = tokenService;
        _activitySource = activitySource;
    }

    public async Task<OperationResult<string>> Register(RegisterModel registerModel)
    {
        using var activity = _activitySource.StartActivity("Register");
        
        _encryptionService.CreatePasswordHash(registerModel.Password, out byte[] passwordHash, out byte[] passwordSalt);

        Credentials credentials = new Credentials()
        {
            Email = registerModel.Email,
            PasswordHash = passwordHash,
            Salt = passwordSalt
        };

        try
        {
            var createdCredentials = await _credentialRepository.RegisterCredentials(credentials);
            string bearerToken = _tokenService.GenerateToken(createdCredentials);

            activity?.Stop();
            return OperationResult<string>.CreateSuccessResult(bearerToken);
        }
        catch (Exception e)
        {
            activity?.Stop();
            return OperationResult<string>.CreateFailure(e.Message);
        }
    }

    public async Task<OperationResult<string>> Login(LoginModel loginModel)
    {
        Credentials existingCredentials = await _credentialRepository.GetCredentialsByEmailAsync(loginModel.Email);
        if (_encryptionService.VerifyPasswordHash(loginModel.Password,existingCredentials.PasswordHash,existingCredentials.Salt))
        {
            string token = _tokenService.GenerateToken(existingCredentials);
            return OperationResult<string>.CreateSuccessResult(token);
        }

        return OperationResult<string>.CreateFailure("Credentials not matching");
    }
}