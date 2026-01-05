using MySqlConnector;

namespace DbProvisioner.Services;

public interface IMySqlDatabaseService
{
    Task<List<string>> GetUsersAsync();
    Task<bool> UserExistsAsync(string username);
    Task<bool> DatabaseExistsAsync(string dbName);
    Task ExecuteNonQueryAsync(string query);
    Task<object?> ExecuteScalarAsync(string query);
}

public class MySqlDatabaseService(string connectionString) : IMySqlDatabaseService
{
    private readonly MySqlDataSource dataSource = new(connectionString);

    public async Task<List<string>> GetUsersAsync()
    {
        var users = new List<string>();
        await using var cmd = dataSource.CreateCommand("SELECT User FROM mysql.user");
        await using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
            users.Add(reader.GetString(0));

        return users;
    }

    public async Task<bool> UserExistsAsync(string username) =>
        await ExecuteScalarAsync($"SELECT 1 FROM mysql.user WHERE User = '{username}'") is not null;

    public async Task<bool> DatabaseExistsAsync(string dbName) =>
        await ExecuteScalarAsync($"SELECT 1 FROM information_schema.schemata WHERE SCHEMA_NAME = '{dbName}'") is not null;

    public async Task ExecuteNonQueryAsync(string query) =>
        await dataSource.CreateCommand(query).ExecuteNonQueryAsync();

    public async Task<object?> ExecuteScalarAsync(string query) =>
        await dataSource.CreateCommand(query).ExecuteScalarAsync();
}