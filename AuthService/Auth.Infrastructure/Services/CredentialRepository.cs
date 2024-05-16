using Auth.Domain.BusinessEntities;
using Auth.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Auth.Infrastructure.Services;

public class CredentialRepository : ICredentialRepository
{
    private readonly string _connectionString;

    public CredentialRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("database")!;
    }

    public async Task<Credentials> RegisterCredentials(Credentials credentials)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            // Check for existing email
            using (var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM Credentials WHERE Email = @Email", conn))
            {
                cmd.Parameters.AddWithValue("@Email", credentials.Email);
                int exists = (int)await cmd.ExecuteScalarAsync();
                if (exists > 0)
                {
                    throw new Exception("Email is already in use");
                }
            }

            // Insert new credentials
            using (var cmd = new NpgsqlCommand("INSERT INTO Credentials (UserId, Email, PasswordHash, Salt) VALUES (@UserId, @Email, @PasswordHash, @Salt)", conn))
            {
                cmd.Parameters.AddWithValue("@UserId", credentials.UserId);
                cmd.Parameters.AddWithValue("@Email", credentials.Email);
                cmd.Parameters.AddWithValue("@PasswordHash", credentials.PasswordHash);
                cmd.Parameters.AddWithValue("@Salt", credentials.Salt);

                await cmd.ExecuteNonQueryAsync();
            }
        }
        return credentials;
    }

    public async Task<Credentials> GetCredentialsByEmailAsync(string email)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (var cmd = new NpgsqlCommand("SELECT * FROM Credentials WHERE Email = @Email", conn))
            {
                cmd.Parameters.AddWithValue("@Email", email);
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    if (!reader.Read())
                    {
                        throw new Exception("Credentials not found for the given email");
                    }

                    return new Credentials
                    {
                        UserId = new Guid((byte[])reader["UserId"]),
                        Email = reader.GetString(reader.GetOrdinal("Email")),
                        PasswordHash = (byte[])reader["PasswordHash"],
                        Salt = (byte[])reader["Salt"]
                    };
                }
            }
        }
    }
}