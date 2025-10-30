using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Services;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // POST /Api/user/register
    [HttpPost("register")]
    public async Task<ActionResult<UserResponseDto>> RegisterUser([FromBody] UpsertUserDto dto)
    {
        try
        {
            var user = await _userService.GetOrCreateUserAsync(dto.TelegramId, dto.Username);

            var response = new UserResponseDto
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering user");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // GET /Api/user/{telegramId}
    [HttpGet("{telegramId}")]
    public async Task<ActionResult<UserResponseDto>> GetUser(long telegramId)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var response = new UserResponseDto
            {
                Id = user.Id,
                TelegramId = user.TelegramId,
                Username = user.Username,
                CreatedAt = user.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}