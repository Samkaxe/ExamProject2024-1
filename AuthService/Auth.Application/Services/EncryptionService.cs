using Auth.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Services;

public class EncryptionService : IEncryptionService
{
    private readonly string? _jwtSecret;

    public EncryptionService(IConfiguration configuration)
    {
        _jwtSecret = configuration?["Secrets:JWT"]; 

    }

    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        string salt = BCrypt.Net.BCrypt.GenerateSalt();

        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(_jwtSecret + password, salt);
        passwordHash = System.Text.Encoding.UTF8.GetBytes(hashedPassword);

        passwordSalt = System.Text.Encoding.UTF8.GetBytes(salt);
    }

    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        return BCrypt.Net.BCrypt.Verify(_jwtSecret + Convert.ToBase64String(storedSalt), Convert.ToBase64String(storedSalt));
    }
}