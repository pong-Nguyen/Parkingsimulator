using Microsoft.Data.Sqlite;
using SmartParkingSystem.Services;

namespace SmartParkingSystem.Data;

public class DatabaseInitializer
{
    private readonly DbConnectionFactory _factory;

    public DatabaseInitializer(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public void Initialize()
    {
        using var connection = _factory.CreateConnection();
        connection.Open();

        ExecuteNonQuery(connection, """
            CREATE TABLE IF NOT EXISTS Users (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                Username TEXT NOT NULL UNIQUE,
                PasswordHash TEXT NOT NULL,
                Role INTEGER NOT NULL,
                FullName TEXT NOT NULL,
                IsActive INTEGER NOT NULL DEFAULT 1
            );

            CREATE TABLE IF NOT EXISTS Vehicles (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                LicensePlate TEXT NOT NULL UNIQUE,
                Type INTEGER NOT NULL
            );

            CREATE TABLE IF NOT EXISTS ParkingTickets (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                VehicleId INTEGER NOT NULL,
                EntryTime TEXT NOT NULL,
                ExitTime TEXT NULL,
                IsPaid INTEGER NOT NULL DEFAULT 0,
                CreatedByUserId INTEGER NOT NULL,
                FOREIGN KEY (VehicleId) REFERENCES Vehicles(Id),
                FOREIGN KEY (CreatedByUserId) REFERENCES Users(Id)
            );

            CREATE TABLE IF NOT EXISTS Payments (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                TicketId INTEGER NOT NULL UNIQUE,
                PaidAt TEXT NOT NULL,
                Amount REAL NOT NULL,
                Method TEXT NOT NULL,
                FOREIGN KEY (TicketId) REFERENCES ParkingTickets(Id)
            );

            CREATE INDEX IF NOT EXISTS IX_Vehicles_LicensePlate ON Vehicles(LicensePlate);
            CREATE INDEX IF NOT EXISTS IX_ParkingTickets_Active ON ParkingTickets(IsPaid, ExitTime);
            CREATE INDEX IF NOT EXISTS IX_Payments_PaidAt ON Payments(PaidAt);
            """);

        SeedUser(connection, "admin", "admin123", 1, "Quan tri vien");
        SeedUser(connection, "staff", "staff123", 2, "Nhan vien");
    }

    private static void SeedUser(SqliteConnection connection, string username, string password, int role, string fullName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO Users (Username, PasswordHash, Role, FullName, IsActive)
            SELECT $username, $hash, $role, $fullName, 1
            WHERE NOT EXISTS (SELECT 1 FROM Users WHERE Username = $username);
            """;
        command.Parameters.AddWithValue("$username", username);
        command.Parameters.AddWithValue("$hash", PasswordHasher.Hash(password));
        command.Parameters.AddWithValue("$role", role);
        command.Parameters.AddWithValue("$fullName", fullName);
        command.ExecuteNonQuery();
    }

    private static void ExecuteNonQuery(SqliteConnection connection, string sql)
    {
        using var command = connection.CreateCommand();
        command.CommandText = sql;
        command.ExecuteNonQuery();
    }
}
