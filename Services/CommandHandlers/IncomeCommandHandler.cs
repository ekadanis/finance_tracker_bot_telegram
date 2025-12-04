using FinanceTracker.Api.Constants;
using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;
using FinanceTracker.Api.Models;
using FinanceTracker.Api.Utils;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class IncomeCommandHandler : ICommandHandler
{
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<IncomeCommandHandler> _logger;

    public string Command => "/in";

    public IncomeCommandHandler(
        ICategoryService categoryService,
        ITransactionService transactionService,
        ILogger<IncomeCommandHandler> logger)
    {
        _categoryService = categoryService;
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleIncomeCommandAsync: {Text}", text);
        
        var (category, note, amount) = CommandParser.ParseTransaction(text);

        if (category == null || amount == null)
        {
            return ErrorMessages.InvalidIncomeFormat;
        }

        var categoryEntity = await _categoryService.GetOrCreateCategoryAsync(userId, category, TransactionType.Income);
        if (categoryEntity == null)
        {
            return ErrorMessages.CategoryCreationFailed;
        }

        await _transactionService.AddTransactionAsync(
            userId, 
            categoryEntity.Id, 
            TransactionType.Income, 
            amount.Value, 
            note ?? "", 
            DateOnly.FromDateTime(DateTime.UtcNow)
        );

        return MessageFormatter.FormatTransactionSuccess("Pemasukan", category, note, amount.Value);
    }
}