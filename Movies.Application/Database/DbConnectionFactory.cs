using System.Data;
using Npgsql;

public interface IDbConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();

    Task<IDbConnection> CreateConnectionAsync(string connectionString);
}

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public NpgsqlConnectionFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task<IDbConnection> CreateConnectionAsync()
    {
        var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();
        return connection;
    }

    public async Task<IDbConnection> CreateConnectionAsync(string connectionString)
    {
        var connection = new NpgsqlConnection(connectionString);
        await connection.OpenAsync();
        return connection;
    }
}