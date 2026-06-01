namespace SmartParkingSystem.Models;

public class DashboardStats
{
    public int TotalSlots { get; set; }
    public int CurrentVehicles { get; set; }
    public int AvailableSlots => Math.Max(0, TotalSlots - CurrentVehicles);
    public decimal RevenueToday { get; set; }
    public decimal RevenueThisWeek { get; set; }
    public decimal RevenueThisMonth { get; set; }
}
