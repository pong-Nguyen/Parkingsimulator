namespace SmartParkingSystem.Models;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public VehicleType Type { get; set; }
}
