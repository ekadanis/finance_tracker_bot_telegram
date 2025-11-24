namespace FinanceTracker.Api.Interfaces;

public interface ICommandHandler
{
    string Command { get; }
    Task<string> HandleAsync(long chatId, string text, Guid userId, string username);
}