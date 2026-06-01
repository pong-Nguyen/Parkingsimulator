namespace SmartParkingSystem.Models;

public class ParkingZone
{
    public string Name { get; set; } = string.Empty;
    public VehicleType VehicleType { get; set; }
    public int Capacity { get; set; }
    public int Occupied { get; set; }
    public int Available => Math.Max(0, Capacity - Occupied);
}
