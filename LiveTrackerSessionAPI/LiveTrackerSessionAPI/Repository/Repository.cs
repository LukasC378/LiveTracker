using Npgsql;

namespace LiveTrackerSessionAPI.Repository;

public class Repository: IRepository
{
    private readonly string _connectionString;

    public Repository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")!;
    }

    public async Task SaveData(int sessionId, string data)
    {
        const string schemaName = "\"Sessions\"";
        var tableName = $"Session_{sessionId}";

        var sql = $@"
            INSERT INTO {schemaName}.{tableName} (datetime, data) VALUES (@datetime, @data)";

        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new NpgsqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("datetime", DateTime.UtcNow);
        cmd.Parameters.AddWithValue("data", data);

        await cmd.ExecuteNonQueryAsync();

        await conn.CloseAsync();
    }
}