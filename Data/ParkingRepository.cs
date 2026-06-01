using Microsoft.Data.Sqlite;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Data;

public class ParkingRepository
{
    private readonly DbConnectionFactory _factory;

    public ParkingRepository(DbConnectionFactory factory)
    {
        _factory = factory;
    }

    public int CountActiveTickets()
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM ParkingTickets WHERE IsPaid = 0 AND ExitTime IS NULL;";
        return Convert.ToInt32(command.ExecuteScalar());
    }

    public bool HasActiveTicket(string licensePlate)
    {
        return FindActiveTicket(licensePlate) is not null;
    }

    public ParkingTicket? FindActiveTicket(string licensePlate)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.Id, v.Id, v.LicensePlate, v.Type, t.EntryTime, t.ExitTime, t.IsPaid, t.CreatedByUserId
            FROM ParkingTickets t
            JOIN Vehicles v ON v.Id = t.VehicleId
            WHERE v.LicensePlate LIKE $plate AND t.IsPaid = 0 AND t.ExitTime IS NULL
            ORDER BY t.EntryTime DESC
            LIMIT 1;
            """;
        command.Parameters.AddWithValue("$plate", NormalizePlate(licensePlate));
        using var reader = command.ExecuteReader();
        return reader.Read() ? MapTicket(reader) : null;
    }

    public List<ParkingTicket> SearchActiveTickets(string keyword)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT t.Id, v.Id, v.LicensePlate, v.Type, t.EntryTime, t.ExitTime, t.IsPaid, t.CreatedByUserId
            FROM ParkingTickets t
            JOIN Vehicles v ON v.Id = t.VehicleId
            WHERE t.IsPaid = 0 AND t.ExitTime IS NULL
              AND v.LicensePlate LIKE $keyword
            ORDER BY t.EntryTime DESC;
            """;
        command.Parameters.AddWithValue("$keyword", $"%{NormalizePlate(keyword)}%");
        return ReadTickets(command);
    }

    public Dictionary<VehicleType, int> CountActiveTicketsByType()
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT v.Type, COUNT(*)
            FROM ParkingTickets t
            JOIN Vehicles v ON v.Id = t.VehicleId
            WHERE t.IsPaid = 0 AND t.ExitTime IS NULL
            GROUP BY v.Type;
            """;

        using var reader = command.ExecuteReader();
        var counts = Enum.GetValues<VehicleType>().ToDictionary(type => type, _ => 0);
        while (reader.Read())
        {
            counts[(VehicleType)reader.GetInt32(0)] = reader.GetInt32(1);
        }

        return counts;
    }

    public int CreateEntryTicket(Vehicle vehicle, DateTime entryTime, int userId)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        var vehicleId = UpsertVehicle(connection, transaction, vehicle);
        using var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = """
            INSERT INTO ParkingTickets (VehicleId, EntryTime, IsPaid, CreatedByUserId)
            VALUES ($vehicleId, $entryTime, 0, $userId);
            SELECT last_insert_rowid();
            """;
        command.Parameters.AddWithValue("$vehicleId", vehicleId);
        command.Parameters.AddWithValue("$entryTime", entryTime.ToString("O"));
        command.Parameters.AddWithValue("$userId", userId);
        var ticketId = Convert.ToInt32(command.ExecuteScalar());
        transaction.Commit();
        return ticketId;
    }

    public void CompleteExit(int ticketId, DateTime exitTime, decimal amount, string method)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        using var update = connection.CreateCommand();
        update.Transaction = transaction;
        update.CommandText = """
            UPDATE ParkingTickets
            SET ExitTime = $exitTime, IsPaid = 1
            WHERE Id = $ticketId AND IsPaid = 0;
            """;
        update.Parameters.AddWithValue("$exitTime", exitTime.ToString("O"));
        update.Parameters.AddWithValue("$ticketId", ticketId);
        update.ExecuteNonQuery();

        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO Payments (TicketId, PaidAt, Amount, Method)
            VALUES ($ticketId, $paidAt, $amount, $method);
            """;
        insert.Parameters.AddWithValue("$ticketId", ticketId);
        insert.Parameters.AddWithValue("$paidAt", exitTime.ToString("O"));
        insert.Parameters.AddWithValue("$amount", amount);
        insert.Parameters.AddWithValue("$method", method);
        insert.ExecuteNonQuery();

        transaction.Commit();
    }

    public decimal GetRevenue(DateTime fromInclusive, DateTime toExclusive)
    {
        using var connection = _factory.CreateConnection();
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT COALESCE(SUM(Amount), 0)
            FROM Payments
            WHERE PaidAt >= $from AND PaidAt < $to;
            """;
        command.Parameters.AddWithValue("$from", fromInclusive.ToString("O"));
        command.Parameters.AddWithValue("$to", toExclusive.ToString("O"));
        return Convert.ToDecimal(command.ExecuteScalar());
    }

    private static int UpsertVehicle(SqliteConnection connection, SqliteTransaction transaction, Vehicle vehicle)
    {
        using var insert = connection.CreateCommand();
        insert.Transaction = transaction;
        insert.CommandText = """
            INSERT INTO Vehicles (LicensePlate, Type)
            VALUES ($plate, $type)
            ON CONFLICT(LicensePlate) DO UPDATE SET Type = excluded.Type;
            """;
        insert.Parameters.AddWithValue("$plate", NormalizePlate(vehicle.LicensePlate));
        insert.Parameters.AddWithValue("$type", (int)vehicle.Type);
        insert.ExecuteNonQuery();

        using var select = connection.CreateCommand();
        select.Transaction = transaction;
        select.CommandText = "SELECT Id FROM Vehicles WHERE LicensePlate = $plate;";
        select.Parameters.AddWithValue("$plate", NormalizePlate(vehicle.LicensePlate));
        return Convert.ToInt32(select.ExecuteScalar());
    }

    private static List<ParkingTicket> ReadTickets(SqliteCommand command)
    {
        using var reader = command.ExecuteReader();
        var tickets = new List<ParkingTicket>();
        while (reader.Read())
        {
            tickets.Add(MapTicket(reader));
        }
        return tickets;
    }

    private static ParkingTicket MapTicket(SqliteDataReader reader)
    {
        return new ParkingTicket
        {
            Id = reader.GetInt32(0),
            Vehicle = new Vehicle
            {
                Id = reader.GetInt32(1),
                LicensePlate = reader.GetString(2),
                Type = (VehicleType)reader.GetInt32(3)
            },
            EntryTime = DateTime.Parse(reader.GetString(4)),
            ExitTime = reader.IsDBNull(5) ? null : DateTime.Parse(reader.GetString(5)),
            IsPaid = reader.GetInt32(6) == 1,
            CreatedByUserId = reader.GetInt32(7)
        };
    }

    private static string NormalizePlate(string plate)
    {
        return plate.Trim().ToUpperInvariant();
    }
}
