using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services;

public class PaymentService
{
    private readonly Dictionary<VehicleType, PricingRule> _pricingRules = new()
    {
        [VehicleType.Bicycle] = new PricingRule { VehicleType = VehicleType.Bicycle, FirstHourFee = 2000, NextHourFee = 1000 },
        [VehicleType.Motorbike] = new PricingRule { VehicleType = VehicleType.Motorbike, FirstHourFee = 5000, NextHourFee = 3000 },
        [VehicleType.Car] = new PricingRule { VehicleType = VehicleType.Car, FirstHourFee = 20000, NextHourFee = 10000 }
    };

    public IReadOnlyDictionary<VehicleType, PricingRule> PricingRules => _pricingRules;

    public decimal CalculateFee(VehicleType type, DateTime entryTime, DateTime exitTime)
    {
        var totalHours = Math.Max(1, (int)Math.Ceiling((exitTime - entryTime).TotalHours));
        var rule = _pricingRules[type];
        return rule.FirstHourFee + Math.Max(0, totalHours - 1) * rule.NextHourFee;
    }

    public void UpdateRule(VehicleType type, decimal firstHourFee, decimal nextHourFee)
    {
        if (firstHourFee < 0 || nextHourFee < 0)
        {
            throw new InvalidOperationException("Gia tien khong duoc am.");
        }

        _pricingRules[type] = new PricingRule
        {
            VehicleType = type,
            FirstHourFee = firstHourFee,
            NextHourFee = nextHourFee
        };
    }
}
