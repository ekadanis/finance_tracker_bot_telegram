using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Interfaces;

public interface IUserService
{
    Task<User?> GetUserByTelegramIdAsync(long telegramId);
    Task<User> GetOrCreateUserAsync(long telegramId, string username);
    Task<List<User>> GetAllUsersAsync();
}