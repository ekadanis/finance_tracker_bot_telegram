    using Telegram.Bot.Types;

    namespace FinanceTracker.Api.Services;

    public interface ITelegramService
    {
        Task SendMessageAsync(long chatId, string message);
        Task ProcessUpdateAsync(Update update);
    }