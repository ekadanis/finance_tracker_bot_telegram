using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _context;
    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, ILogger<UserService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<User?> GetUserByTelegramIdAsync(long telegramId)
    {
        return await _context.Users
            .FirstOrDefaultAsync(user => user.TelegramId == telegramId);
    }

    public async Task<User> GetOrCreateUserAsync(long telegramId, string username)
    {
        var user = await GetUserByTelegramIdAsync(telegramId);

        if (user == null)
        {
            user = new User
            {
                TelegramId = telegramId,
                Username = username
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation("New user created: {Username} (TelegramId: {TelegramId})", username, telegramId);
        }
        else if (user.Username != username)
        {
            user.Username = username;
            await _context.SaveChangesAsync();
            _logger.LogInformation("Username updated for TelegramId {TelegramId}: {Username}", telegramId, username);
        }

        return user;
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        return await _context.Users.ToListAsync();
    }
}