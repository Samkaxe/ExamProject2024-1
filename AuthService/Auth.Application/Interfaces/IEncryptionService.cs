namespace Auth.Application.Interfaces;

public interface IEncryptionService
{
    public void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt);

    public bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt);

}