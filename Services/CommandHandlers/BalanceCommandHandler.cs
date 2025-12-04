using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class BalanceCommandHandler : ICommandHandler
{
    private readonly ITransactionService _transactionService;
    private readonly ILogger<BalanceCommandHandler> _logger;

    public string Command => "/saldo";

    public BalanceCommandHandler(
        ITransactionService transactionService,
        ILogger<BalanceCommandHandler> logger)
    {
        _transactionService = transactionService;
        _logger = logger;
    }

    public async Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleBalanceCommandAsync for user {UserId}", userId);

        var (income, expense) = await _transactionService.GetTotalIncomeExpenseAsync(userId);
        var balance = income - expense;

        return MessageFormatter.FormatBalance(balance, income, expense);
    }
}