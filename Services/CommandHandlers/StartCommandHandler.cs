using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Interfaces;

namespace FinanceTracker.Api.Services.CommandHandlers;

public class StartCommandHandler : ICommandHandler
{
    private readonly ILogger<StartCommandHandler> _logger;

    public string Command => "/start";

    public StartCommandHandler(ILogger<StartCommandHandler> logger)
    {
        _logger = logger;
    }

    public Task<string> HandleAsync(long chatId, string text, Guid userId, string username)
    {
        _logger.LogInformation("HandleStartCommandAsync for {Username}", username);
        var message = MessageFormatter.GetWelcomeMessage(username);
        return Task.FromResult(message);
    }
}