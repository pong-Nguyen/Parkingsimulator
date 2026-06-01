using SmartParkingSystem.Data;
using SmartParkingSystem.Forms;
using SmartParkingSystem.Services;

namespace SmartParkingSystem;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        const int totalSlots = 50;
        var databasePath = Path.Combine(AppContext.BaseDirectory, "Data", "smart_parking.db");
        var connectionFactory = new DbConnectionFactory(databasePath);
        new DatabaseInitializer(connectionFactory).Initialize();

        var authRepository = new AuthRepository(connectionFactory);
        var parkingRepository = new ParkingRepository(connectionFactory);
        var authService = new AuthService(authRepository);
        var paymentService = new PaymentService();
        var parkingLot = new ParkingLot(parkingRepository, paymentService, totalSlots);
        var statisticsService = new StatisticsService(parkingRepository, parkingLot);

        Application.Run(new LoginForm(authService, parkingLot, statisticsService));
    }    
}
