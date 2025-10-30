using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Utils;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Services;

public interface ITelegramService
{
    Task SendMessageAsync(long chatId, string message);
    Task ProcessUpdateAsync(Update update);
}