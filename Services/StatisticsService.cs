using SmartParkingSystem.Data;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services;

public class StatisticsService
{
    private readonly ParkingRepository _repository;
    private readonly int _totalSlots;

    public StatisticsService(ParkingRepository repository, int totalSlots)
    {
        _repository = repository;
        _totalSlots = totalSlots;
    }

    public DashboardStats GetDashboardStats()
    {
        var today = DateTime.Today;
        var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
        if (today.DayOfWeek == DayOfWeek.Sunday)
        {
            startOfWeek = today.AddDays(-6);
        }

        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        return new DashboardStats
        {
            TotalSlots = _totalSlots,
            CurrentVehicles = _repository.CountActiveTickets(),
            RevenueToday = _repository.GetRevenue(today, today.AddDays(1)),
            RevenueThisWeek = _repository.GetRevenue(startOfWeek, startOfWeek.AddDays(7)),
            RevenueThisMonth = _repository.GetRevenue(startOfMonth, startOfMonth.AddMonths(1))
        };
    }
}
