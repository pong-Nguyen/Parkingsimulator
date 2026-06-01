using Microsoft.Data.Sqlite;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Data;

public class AuthRepository
{
    private readonly DbConnectionFactory _factory;

    public AuthRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public User? FindActiveUser(string username)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT Id, Username, PasswordHash, Role, FullName, IsActive
            FROM Users
            WHERE Username = $username AND IsActive = 1;
            """;
        command.Parameters.AddWithValue("$username", username.Trim());

        using var reader = command.ExecuteReader();
        if (!reader.Read())
        {
            return null;
        }

        return new User
        {
            Id = reader.GetInt32(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2),
            Role = (UserRole)reader.GetInt32(3),
            FullName = reader.GetString(4),
            IsActive = reader.GetInt32(5) == 1
        };
    }
}
