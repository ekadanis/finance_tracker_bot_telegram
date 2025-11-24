using Microsoft.AspNetCore.Mvc;
using Telegram.Bot;
using Telegram.Bot.Types;
using System.Text.Json;
using FinanceTracker.Api.Interfaces;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramBotClient _botClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<TelegramController> _logger;
    private readonly HttpClient _httpClient;
    private readonly ITelegramService _telegramService;

    public TelegramController(
        ITelegramBotClient botClient, 
        IConfiguration configuration,
        ILogger<TelegramController> logger,
        IHttpClientFactory httpClientFactory,
        ITelegramService telegramService)
    {
        _botClient = botClient;
        _configuration = configuration;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient();
        _telegramService = telegramService;
    }

    // POST /Api/Telegram/webhook
    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] JsonElement updateJson)
    {    
        try
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
            };
            
            var update = JsonSerializer.Deserialize<Update>(updateJson.GetRawText(), options);
            
            if (update == null)
            {
                _logger.LogWarning("Failed to deserialize update");
                return BadRequest(new { error = "Invalid update format" });
            }
            
            if (update.Message != null)
            {
                _logger.LogInformation("Message Text: {Text}", update.Message.Text);
                _logger.LogInformation("From User: {Username} ({TelegramId})", 
                    update.Message.From?.Username, 
                    update.Message.From?.Id);
            }
            
            await _telegramService.ProcessUpdateAsync(update);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "=== WEBHOOK PROCESSING FAILED ===");
            return Ok(); 
        }
    }

    // POST /Api/Telegram/setwebhook
    [HttpPost("setwebhook")]
    public async Task<IActionResult> SetWebhook()
    {
        try
        {
            var webhookUrl = _configuration["TelegramSettings:WebhookUrl"];
            var botToken = _configuration["TelegramSettings:BotToken"];

            var url = $"https://api.telegram.org/bot{botToken}/setWebhook?drop_pending_updates=true";
            var payload = new { url = webhookUrl };
            var content = new StringContent(
                JsonSerializer.Serialize(payload), 
                System.Text.Encoding.UTF8, 
                "application/json");

            var response = await _httpClient.PostAsync(url, content);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Webhook set to: {WebhookUrl}", webhookUrl);
            _logger.LogInformation("Telegram API Response: {Response}", responseBody);
            
            return Ok(new 
            { 
                success = true, 
                message = "Webhook set successfully",
                webhookUrl = webhookUrl,
                telegramResponse = JsonSerializer.Deserialize<object>(responseBody)
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting webhook");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    [HttpGet("webhookinfo")]
    public async Task<IActionResult> GetWebhookInfo()
    {
        try
        {
            var botToken = _configuration["TelegramSettings:BotToken"];
            
            if (string.IsNullOrWhiteSpace(botToken))
            {
                return BadRequest(new { error = "BotToken not configured in appsettings.json" });
            }

            var url = $"https://api.telegram.org/bot{botToken}/getWebhookInfo";
            var response = await _httpClient.GetAsync(url);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Webhook info retrieved");
            
            return Ok(JsonSerializer.Deserialize<object>(responseBody));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting webhook info");
            return StatusCode(500, new { error = ex.Message });
        }
    }

    // POST /Api/Telegram/deletewebhook 
    [HttpPost("deletewebhook")]
    public async Task<IActionResult> DeleteWebhook()
    {
        try
        {
            var botToken = _configuration["TelegramSettings:BotToken"];
            
            if (string.IsNullOrWhiteSpace(botToken))
            {
                return BadRequest(new { error = "BotToken not configured in appsettings.json" });
            }

            // Use Telegram API directly
            var url = $"https://api.telegram.org/bot{botToken}/deleteWebhook";
            var response = await _httpClient.PostAsync(url, null);
            var responseBody = await response.Content.ReadAsStringAsync();
            
            _logger.LogInformation("Webhook deleted");
            
            return Ok(JsonSerializer.Deserialize<object>(responseBody));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting webhook");
            return StatusCode(500, new { error = ex.Message });
        }
    }
}