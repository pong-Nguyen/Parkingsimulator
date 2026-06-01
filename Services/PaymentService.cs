using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services;

public class PaymentService
{
    public decimal CalculateFee(VehicleType type, DateTime entryTime, DateTime exitTime)
    {
        var totalHours = Math.Max(1, (int)Math.Ceiling((exitTime - entryTime).TotalHours));
        return type switch
        {
            VehicleType.Bicycle => 2000 + Math.Max(0, totalHours - 1) * 1000,
            VehicleType.Motorbike => 5000 + Math.Max(0, totalHours - 1) * 3000,
            VehicleType.Car => 20000 + Math.Max(0, totalHours - 1) * 10000,
            _ => 0
        };
    }
}
