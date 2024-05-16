using Auth.Domain.BusinessEntities;

namespace Auth.Application.Interfaces;

public interface ITokenService
{
    public string GenerateToken(Credentials credentials);
}