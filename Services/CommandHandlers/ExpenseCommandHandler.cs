using FinanceTracker.Api.Constants;
using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Utils;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class ExpenseCommandHandler : ICommandHandler
{
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<ExpenseCommandHandler> _logger;

    public string Command => "/out";

    public ExpenseCommandHandler(
        ICategoryService categoryService,
        ITransactionService transactionService,
        ILogger<ExpenseCommandHandler> logger)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleExpenseCommandAsync: {Text}", text);
        
        var (category, note, amount) = CommandParser.ParseTransaction(text);

        if (category == null || amount == null)
        {
            return ErrorMessages.InvalidExpenseFormat;
        }

        var categoryEntity = await _categoryService.GetOrCreateCategoryAsync(userId, category, TransactionType.Expense);
        if (categoryEntity == null)
        {
            return ErrorMessages.CategoryCreationFailed;
        }

        await _transactionService.AddTransactionAsync(
            userId, 
            categoryEntity.Id, 
            TransactionType.Expense, 
            amount.Value, 
            note ?? "", 
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        return MessageFormatter.FormatTransactionSuccess("Pengeluaran", category, note, amount.Value);
    }
}