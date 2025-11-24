using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Interfaces;

public interface ITransactionService
{
    Task<Transaction> AddTransactionAsync(Guid userId, Guid categoryId, TransactionType type, decimal amount, string note, DateOnly date);
    Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);
    Task<decimal> GetBalanceAsync(Guid userId);
    Task<(decimal Income, decimal Expense)> GetTotalIncomeExpenseAsync(Guid userId);
    Task<(decimal Income, decimal Expense, decimal Balance)> GetRecapAsync(Guid userId, DateOnly startDate, DateOnly endDate);
    Task<List<Transaction>> GetTransactionsByPeriodAsync(Guid userId, DateOnly startDate, DateOnly endDate);
}
