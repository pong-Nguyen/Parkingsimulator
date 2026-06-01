using SmartParkingSystem.Data;
using SmartParkingSystem.Models;

namespace SmartParkingSystem.Services;

public class AuthService
{
    private readonly AuthRepository _repository;

    public AuthService(AuthRepository repository)
    {
        _repository = repository;
    }

    public User? Login(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            return null;
        }

        var user = _repository.FindActiveUser(username);
        return user is not null && user.PasswordHash == PasswordHasher.Hash(password) ? user : null;
    }
}
