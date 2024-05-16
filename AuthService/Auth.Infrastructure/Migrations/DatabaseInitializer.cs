using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Auth.Infrastructure.Migrations;

public class DatabaseInitializer
{
    private readonly string _connectionString;

    public DatabaseInitializer(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("database")!;
    }

    public async Task InitializeAsync()
    {
        await EnsureDatabaseCreatedAsync();
        await EnsureCredentialsTableCreatedAsync();
    }

    private async Task EnsureDatabaseCreatedAsync()
    {
        var builder = new NpgsqlConnectionStringBuilder(_connectionString);
        var databaseName = builder.Database;
        builder.Database = "postgres"; // Default database to connect to

        using (var conn = new NpgsqlConnection(builder.ConnectionString))
        {
            await conn.OpenAsync();
            var exists = await new NpgsqlCommand($"SELECT 1 FROM pg_database WHERE datname='{databaseName}'", conn).ExecuteScalarAsync();
            if (exists == null)
            {
                await new NpgsqlCommand($"CREATE DATABASE \"{databaseName}\"", conn).ExecuteNonQueryAsync();
            }
        }
    }

    private async Task EnsureCredentialsTableCreatedAsync()
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            var cmd = new NpgsqlCommand(@"
                CREATE TABLE IF NOT EXISTS Credentials (
                    UserId UUID PRIMARY KEY DEFAULT gen_random_uuid(),
                    Email VARCHAR(256) NOT NULL UNIQUE,
                    PasswordHash BYTEA NOT NULL,
                    Salt BYTEA NOT NULL
                );", conn);
            await cmd.ExecuteNonQueryAsync();
        }
    }
}