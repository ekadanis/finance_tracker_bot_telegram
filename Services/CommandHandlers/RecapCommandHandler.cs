using FinanceTracker.Api.Constants;
using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Utils;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class RecapCommandHandler : ICommandHandler
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<RecapCommandHandler> _logger;

    public string Command => "/recap";

    public RecapCommandHandler(
        ITransactionService transactionService,
        ILogger<RecapCommandHandler> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleRecapCommandAsync: {Text}", text);

        var (startDate, endDate) = CommandParser.ParseRecapPeriod(text);
        
         // ✅ Tambahkan logging ini untuk debug
        _logger.LogInformation("Parsed dates - StartDate: {StartDate}, EndDate: {EndDate}", 
        startDate?.ToString() ?? "NULL", 
        endDate?.ToString() ?? "NULL");
    

        if (startDate == null || endDate == null)
        {
            _logger.LogWarning("Date parsing failed for text: {Text}", text);
            return ErrorMessages.InvalidRecapFormat;
        }

        var transactions = await _transactionService.GetTransactionsByPeriodAsync(userId, startDate.Value, endDate.Value);

        if (transactions.Count == 0)
        {
            return MessageFormatter.GetErrorMessage();
        }

        var income = transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
        var expense = transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
        var balance = income - expense;

        return MessageFormatter.FormatRecap(startDate.Value, endDate.Value, income, expense, balance, transactions);
    }
}