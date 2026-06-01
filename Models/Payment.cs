namespace SmartParkingSystem.Models;

public class Payment
{
    public int Id { get; set; }
    public int TicketId { get; set; }
    public DateTime PaidAt { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = "Cash";
}
