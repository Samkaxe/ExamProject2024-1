using System.Security.Cryptography;
using System.Text;
using Auth.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace Auth.Application.Services;

public class EncryptionService : IEncryptionService
{
    private readonly string? _jwtSecret;

    public EncryptionService(IConfiguration configuration)
    {
        _jwtSecret = configuration["Secrets:JWT"]; 

    }

    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
    {
        // Generate a random salt
        string salt = BCrypt.Net.BCrypt.GenerateSalt();
        passwordSalt = Encoding.UTF8.GetBytes(salt);

        // Using HMAC to hash the password first
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_jwtSecret));
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hmacBytes = hmac.ComputeHash(passwordBytes);

        // Now hash the HMAC output with BCrypt along with the salt
        string hashedPassword = BCrypt.Net.BCrypt.HashPassword(Convert.ToBase64String(hmacBytes), salt);
        passwordHash = Encoding.UTF8.GetBytes(hashedPassword);
    }

    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
    {
        // Decode the stored salt from byte array to string
        string saltString = Encoding.UTF8.GetString(storedSalt);

        // Using HMAC to hash the input password first
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_jwtSecret));
        var passwordBytes = Encoding.UTF8.GetBytes(password);
        var hmacBytes = hmac.ComputeHash(passwordBytes);

        // Convert the HMAC output to a base64 string to hash with BCrypt using the stored salt
        string hmacOutput = Convert.ToBase64String(hmacBytes);

        // Rehash the HMAC output with the original salt used during hashing
        string rehashedPassword = BCrypt.Net.BCrypt.HashPassword(hmacOutput, saltString);

        // Convert stored hash from byte array to string for comparison
        string storedHashString = Encoding.UTF8.GetString(storedHash);

        // Verify if the rehashed password with the salt matches the stored hash
        return rehashedPassword == storedHashString;
    }

}