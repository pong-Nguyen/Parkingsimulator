using SmartParkingSystem.Data;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services;

public class ParkingLot
{
    private readonly ParkingRepository _repository;
    private readonly PaymentService _paymentService;

    public ParkingLot(ParkingRepository repository, PaymentService paymentService, int totalSlots)
    {
        _repository = repository;
        _paymentService = paymentService;
        TotalSlots = totalSlots;
    }

    public int TotalSlots { get; }

    public bool IsFull => _repository.CountActiveTickets() >= TotalSlots;

    public int CheckIn(string licensePlate, VehicleType type, DateTime entryTime, int userId)
    {
        if (IsFull)
        {
            throw new InvalidOperationException("Bai xe da day, khong the nhan them xe.");
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
}
