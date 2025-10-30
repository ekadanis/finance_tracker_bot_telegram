using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly AppDbContext _context;

    public TransactionService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Transaction> AddTransactionAsync(Guid userId, Guid categoryId, TransactionType type, decimal amount, string note, DateTime date)
    {
        var transaction = new Transaction
        {
            UserId = userId,
            CategoryId = categoryId,
            Type = type,
            Amount = amount,
            Note = note,
            Date = date
        };

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();
        return transaction;
    }

    public async Task<Transaction?> GetTransactionByIdAsync(Guid transactionId)
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == transactionId);
    }

    public async Task<decimal> GetBalanceAsync(Guid userId)
    {
        var income = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expense = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return income - expense;
    }

    public async Task<(decimal Income, decimal Expense)> GetTotalIncomeExpenseAsync(Guid userId)
    {
        var income = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Income)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        var expense = await _context.Transactions
            .Where(t => t.UserId == userId && t.Type == TransactionType.Expense)
            .SumAsync(t => (decimal?)t.Amount) ?? 0;

        return (income, expense);
    }

    public async Task<(decimal Income, decimal Expense, decimal Balance)> GetRecapAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        var transactions = await _context.Transactions
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .ToListAsync();

        var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);

        return (income, expense, income - expense);
    }

    public async Task<List<Transaction>> GetTransactionsByPeriodAsync(Guid userId, DateTime startDate, DateTime endDate)
    {
        return await _context.Transactions
            .Include(t => t.User)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Date >= startDate && t.Date <= endDate)
            .OrderByDescending(t => t.Date)
            .ToListAsync();
    }
}