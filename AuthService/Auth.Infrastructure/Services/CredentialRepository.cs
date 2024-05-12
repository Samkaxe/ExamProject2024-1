using Auth.Domain.BusinessEntities;
using Auth.Infrastructure.Database;
using Auth.Infrastructure.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Services;

public class CredentialRepository: ICredentialRepository
{
    private readonly DatabaseContext _databaseContext;

    public CredentialRepository(DatabaseContext databaseContext)
    {
        _databaseContext = databaseContext;
        _databaseContext.Database.EnsureCreatedAsync();
    }

    public async Task<Credentials> RegisterCredentials(Credentials credentials)
    {
        // Check for existing email (using the DbSet)
        if (await _databaseContext.Credentials.AnyAsync(c => c.Email == credentials.Email)) 
        {
            throw new Exception("Email is already in use");
        }

        // Add to the DbSet
        await _databaseContext.Credentials.AddAsync(credentials);
        await _databaseContext.SaveChangesAsync(); // Save changes to the database
        return credentials;
    }

    public async Task<Credentials> GetCredentialsByEmailAsync(string email)
    {
        // Get credentials from the DbSet
        var credentials = await _databaseContext.Credentials
            .FirstOrDefaultAsync(c => c.Email == email);

        if (credentials == null)
        {
            throw new Exception("Credentials not found for the given email");
        }

        return credentials;
    }
}