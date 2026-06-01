using SmartParkingSystem.Data;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services;

public class ParkingLot
{
    private readonly ParkingRepository _repository;
    private readonly PaymentService _paymentService;
    private readonly Dictionary<VehicleType, ParkingZone> _zones;

    public ParkingLot(ParkingRepository repository, PaymentService paymentService, int totalSlots)
    {
        _repository = repository;
        _paymentService = paymentService;
        _zones = new Dictionary<VehicleType, ParkingZone>
        {
            [VehicleType.Bicycle] = new ParkingZone { Name = "Khu A - Xe dap", VehicleType = VehicleType.Bicycle, Capacity = 8 },
            [VehicleType.Motorbike] = new ParkingZone { Name = "Khu B - Xe may", VehicleType = VehicleType.Motorbike, Capacity = 30 },
            [VehicleType.Car] = new ParkingZone { Name = "Khu C - O to", VehicleType = VehicleType.Car, Capacity = 12 }
        };
    }

    public int TotalSlots => _zones.Values.Sum(zone => zone.Capacity);
    public PaymentService PaymentService => _paymentService;

    public bool IsFull => _repository.CountActiveTickets() >= TotalSlots;

    public int CheckIn(string licensePlate, VehicleType type, DateTime entryTime, int userId)
    {
        if (IsFull)
        {
            throw new InvalidOperationException("Bai xe da day, khong the nhan them xe.");
        }

        var zone = GetZones().First(z => z.VehicleType == type);
        if (zone.Available <= 0)
        {
            throw new InvalidOperationException($"{zone.Name} da day, khong the nhan them xe loai nay.");
        }

        if (_repository.HasActiveTicket(licensePlate))
        {
            throw new InvalidOperationException("Bien so nay dang co trong bai.");
        }

        return _repository.CreateEntryTicket(new Vehicle
        {
            LicensePlate = licensePlate,
            Type = type
        }, entryTime, userId);
    }

    public (ParkingTicket Ticket, decimal Fee) PreviewExit(string licensePlate, DateTime exitTime)
    {
        var ticket = _repository.FindActiveTicket(licensePlate)
            ?? throw new InvalidOperationException("Khong tim thay xe dang gui voi bien so nay.");
        var fee = _paymentService.CalculateFee(ticket.Vehicle.Type, ticket.EntryTime, exitTime);
        return (ticket, fee);
    }

    public decimal CheckOut(string licensePlate, DateTime exitTime, string method)
    {
        var (ticket, fee) = PreviewExit(licensePlate, exitTime);
        _repository.CompleteExit(ticket.Id, exitTime, fee, method);
        return fee;
    }

    public List<ParkingTicket> SearchActiveTickets(string keyword)
    {
        return _repository.SearchActiveTickets(keyword);
    }

    public List<ParkingZone> GetZones()
    {
        var counts = _repository.CountActiveTicketsByType();
        return _zones.Values
            .Select(zone => new ParkingZone
            {
                Name = zone.Name,
                VehicleType = zone.VehicleType,
                Capacity = zone.Capacity,
                Occupied = counts.GetValueOrDefault(zone.VehicleType)
            })
            .ToList();
    }

    public void UpdateZoneCapacity(VehicleType type, int capacity)
    {
        if (capacity < 0)
        {
            throw new InvalidOperationException("Suc chua khong duoc am.");
        }

        var occupied = _repository.CountActiveTicketsByType().GetValueOrDefault(type);
        if (capacity < occupied)
        {
            throw new InvalidOperationException($"Suc chua moi phai lon hon hoac bang so xe hien co ({occupied}).");
        }

        _zones[type].Capacity = capacity;
    }
}
