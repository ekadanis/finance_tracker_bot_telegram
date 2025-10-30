using FinanceTracker.Api.Data;
using FinanceTracker.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Api.Services;

public interface ITransactionService
{
    Task<Transaction> AddTransactionAsync(Guid userId, Guid categoryId, TransactionType type, decimal amount, string note, DateTime date);
    Task<Transaction?> GetTransactionByIdAsync(Guid transactionId);
    Task<decimal> GetBalanceAsync(Guid userId);
    Task<(decimal Income, decimal Expense)> GetTotalIncomeExpenseAsync(Guid userId);
    Task<(decimal Income, decimal Expense, decimal Balance)> GetRecapAsync(Guid userId, DateTime startDate, DateTime endDate);
    Task<List<Transaction>> GetTransactionsByPeriodAsync(Guid userId, DateTime startDate, DateTime endDate);
}
