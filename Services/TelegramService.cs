using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using FinanceTracker.Api.Helpers;
using FinanceTracker.Api.Constants;
using FinanceTracker.Api.Interfaces;
using System.Text.Json;

namespace FinanceTracker.Api.Services;

public class TelegramService : ITelegramService 
{
    private readonly string _botToken;
    private readonly IUserService _userService;
    private readonly IEnumerable<ICommandHandler> _commandHandlers;
    private readonly ILogger<TelegramService> _logger;
    private readonly HttpClient _httpClient;

    public TelegramService(
        ITelegramBotClient botClient,
        IConfiguration configuration,
        IUserService userService,
        IEnumerable<ICommandHandler> commandHandlers,
        ILogger<TelegramService> logger,
        IHttpClientFactory httpClientFactory)
    {
        _botToken = configuration["TelegramSettings:BotToken"] ?? throw new ArgumentNullException("BotToken");
        _userService = userService;
        _commandHandlers = commandHandlers;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
    }

    public async Task SendMessageAsync(long chatId, string message)
    {
        try 
        {
            var url = $"https://api.telegram.org/bot{_botToken}/sendMessage";
            var payload = new
            {
                chat_id = chatId,
                text = message,
                parse_mode = "Markdown"
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                System.Text.Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send message. Status: {Status}, Response: {Response}", 
                    response.StatusCode, errorBody);
            }
        } 
        catch (Exception ex) 
        {
            _logger.LogError(ex, "Exception when sending message to chat {ChatId}", chatId);
        }
    }

    public async Task ProcessUpdateAsync(Update update)
    {
        _logger.LogInformation("ProcessUpdateAsync called. Update Type: {Type}", update.Type);
        
        if (update.Type != UpdateType.Message || update.Message?.Text == null)
        {
            _logger.LogWarning("Update ignored. Type: {Type}, HasMessage: {HasMessage}, HasText: {HasText}", 
                update.Type, 
                update.Message != null, 
                update.Message?.Text != null);
            return;
        }

        var message = update.Message;
        var chatId = message.Chat.Id;
        var telegramId = message.From?.Id ?? 0;
        var text = message.Text;
        var username = message.From?.Username ?? $"User_{telegramId}";

        _logger.LogInformation("Processing message from {Username} ({TelegramId}): {Text}", 
            username, telegramId, text);

        try
        {
            var user = await _userService.GetOrCreateUserAsync(telegramId, username);
            
            var handler = _commandHandlers.FirstOrDefault(h => text.StartsWith(h.Command));
            
            string responseMessage;
            if (handler != null)
            {
                responseMessage = await handler.HandleAsync(chatId, text, user.Id, username);
            }
            else
            {
                responseMessage = MessageFormatter.GetInvalidFormatMessage();
            }

            await SendMessageAsync(chatId, responseMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing update for user {TelegramId}", telegramId);
            await SendMessageAsync(chatId, ErrorMessages.GenericError);
        }
    }
}