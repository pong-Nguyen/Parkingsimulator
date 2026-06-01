namespace SmartParkingSystem.Models;

public class PricingRule
{
    public VehicleType VehicleType { get; set; }
    public decimal FirstHourFee { get; set; }
    public decimal NextHourFee { get; set; }
}
