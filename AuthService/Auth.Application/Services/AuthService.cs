using Auth.API.DTOs;
using Auth.Application.Interfaces;
using Auth.Domain.OperationResults;
using Auth.Domain.Security;

namespace Auth.Application.Services;

public class AuthService: IAuthService
{
    public Task<OperationResult<BearerToken>> Register(RegisterModel registerModel)
    {
        throw new NotImplementedException();
    }

    public Task<OperationResult<BearerToken>> Login(LoginModel loginModel)
    {
        throw new NotImplementedException();
    }
}