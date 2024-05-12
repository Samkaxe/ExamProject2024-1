using Auth.Application.DTOs;
using Auth.Domain.OperationResults;

namespace Auth.Application.Interfaces;

public interface IAuthService
{
    public Task<OperationResult<string>> Register(RegisterModel registerModel);

    public Task<OperationResult<string>> Login(LoginModel loginModel);
}