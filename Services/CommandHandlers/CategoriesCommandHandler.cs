using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class CategoriesCommandHandler : ICommandHandler
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesCommandHandler> _logger;

    public string Command => "/categories";

    public CategoriesCommandHandler(
        ICategoryService categoryService,
        ILogger<CategoriesCommandHandler> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleCategoriesCommandAsync for user {UserId}", userId);

        var incomeCategories = await _categoryService.GetUserCategoriesAsync(userId, TransactionType.Income);
        var expenseCategories = await _categoryService.GetUserCategoriesAsync(userId, TransactionType.Expense);

        return MessageFormatter.FormatCategories(incomeCategories, expenseCategories);
    }
}