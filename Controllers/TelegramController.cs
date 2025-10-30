using Microsoft.AspNetCore.Mvc;
using Telegram.Bot.Types;
using FinanceTracker.Api.Services;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class TelegramController : ControllerBase
{
    private readonly ITelegramService _telegramService;
    private readonly ILogger<TelegramController> _logger;

    public TelegramController(ITelegramService telegramService, ILogger<TelegramController> logger)
    {
        _telegramService = telegramService;
        _logger = logger;
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> Webhook([FromBody] Update update)
    {
        try
        {
            await _telegramService.ProcessUpdateAsync(update);
            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing webhook update");
            return StatusCode(500);
        }
    }

    [HttpGet("health")]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}