using Auth.API.DTOs;
using Auth.Domain.OperationResults;
using Auth.Domain.Security;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    public Task<OperationResult<BearerToken>> Register(RegisterModel registerModel);

    public Task<OperationResult<BearerToken>> Login(LoginModel loginModel);
}