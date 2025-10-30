using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Services;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class CategoryController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoryController> _logger;

    public CategoryController(
        IUserService userService,
        ICategoryService categoryService,
        ILogger<CategoryController> logger)
    {
        _userService = userService;
        _categoryService = categoryService;
        _logger = logger;
    }

    // POST /Api/category/list
    [HttpPost("list")]
    public async Task<ActionResult<List<CategoryResponseDto>>> GetCategories([FromBody] ListCategoriesRequestDto dto)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(dto.TelegramId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var categories = await _categoryService.GetUserCategoriesAsync(user.Id, dto.Type);

            var response = categories.Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Type = c.Type,
                CreatedAt = c.CreatedAt
            }).ToList();

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting categories");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}