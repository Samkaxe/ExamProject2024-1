using Auth.Infrastructure.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;

namespace Auth.Infrastructure.Services;

public class PostgresHealthCheck: IDatabaseHealthCheck
{
    private readonly string _connectionString;

    public PostgresHealthCheck(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("database")!;
    }

    public async Task<bool> IsHealthyAsync()
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            if (conn.State != System.Data.ConnectionState.Open) 
            {
                await conn.OpenAsync();
            }

            using (var cmd = new NpgsqlCommand("SELECT 1", conn))
            {
                await cmd.ExecuteScalarAsync();
            }
        }
        catch (Exception ex)
        {
            return false;
        }

        return true;
    }
}