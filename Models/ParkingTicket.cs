namespace SmartParkingSystem.Models;

public class ParkingTicket
{
    public int Id { get; set; }
    public Vehicle Vehicle { get; set; } = new();
    public DateTime EntryTime { get; set; }
    public DateTime? ExitTime { get; set; }
    public bool IsPaid { get; set; }
    public int CreatedByUserId { get; set; }

    public TimeSpan Duration => (ExitTime ?? DateTime.Now) - EntryTime;
}
