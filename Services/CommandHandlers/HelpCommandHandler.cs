using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class HelpCommandHandler : ICommandHandler
{
    private readonly ILogger<HelpCommandHandler> _logger;

    public string Command => "/help";

    public HelpCommandHandler(ILogger<HelpCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleHelpCommandAsync");
        var message = MessageFormatter.GetHelpMessage();
        return Task.FromResult(message);
    }
}