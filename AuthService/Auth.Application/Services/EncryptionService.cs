using System.Text;
using Auth.Application.Interfaces;
using BCrypt.Net;
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
        // Generate a salt and hash the password with the secret appended if needed
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(_jwtSecret + password, salt);

        // Convert the hashed password to a byte array
        passwordHash = System.Text.Encoding.UTF8.GetBytes(hashedPassword);

        // Extract just the salt part of the hash to show how it can be done, though it's not necessary
        string extractedSalt = salt; // BCrypt salt can be extracted from hashedPassword if needed
        passwordSalt = System.Text.Encoding.UTF8.GetBytes(extractedSalt);
    }

    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        // Convert the stored hash from bytes back to string
        string hashString = System.Text.Encoding.UTF8.GetString(storedHash);

        // Verify the password (with secret appended) against the stored hash
        return BCrypt.Net.BCrypt.Verify(_jwtSecret + password, hashString);
    }

}