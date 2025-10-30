using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Services;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.Controllers;

[ApiController]
[Route("Api/[controller]")]
public class TransactionController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ICategoryService _categoryService;
    private readonly ITransactionService _transactionService;
    private readonly ILogger<TransactionController> _logger;

    public TransactionController(
        IUserService userService,
        ICategoryService categoryService,
        ITransactionService transactionService,
        ILogger<TransactionController> logger)
    {
        _userService = userService;
        _categoryService = categoryService;
        _transactionService = transactionService;
        _logger = logger;
    }

    // POST /Api/transaction
    [HttpPost]
    public async Task<ActionResult<TransactionResponseDto>> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        try
        {
            // Get or create user
            var user = await _userService.GetUserByTelegramIdAsync(dto.TelegramId);
            if (user == null)
            {
                return BadRequest(new { error = "User not found. Please register first." });
            }

            // Get or create category
            var category = await _categoryService.GetOrCreateCategoryAsync(user.Id, dto.CategoryName, dto.Type);
            if (category == null)
            {
                return BadRequest(new { error = "Failed to get or create category" });
            }

            // Create transaction
            var transactionDate = dto.Date ?? DateTime.UtcNow;
            var transaction = await _transactionService.AddTransactionAsync(
                user.Id,
                category.Id,
                dto.Type,
                dto.Amount,
                dto.Note,
                transactionDate
            );

            var response = new TransactionResponseDto
            {
                Id = transaction.Id,
                Username = user.Username,
                CategoryName = category.Name,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Note = transaction.Note,
                Date = transaction.Date,
                CreatedAt = transaction.CreatedAt
            };

            return CreatedAtAction(nameof(GetTransaction), new { id = transaction.Id }, response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // GET /Api/transaction/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<TransactionResponseDto>> GetTransaction(Guid id)
    {
        try
        {
            var transaction = await _transactionService.GetTransactionByIdAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            var response = new TransactionResponseDto
            {
                Id = transaction.Id,
                Username = transaction.User.Username,
                CategoryName = transaction.Category.Name,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Note = transaction.Note,
                Date = transaction.Date,
                CreatedAt = transaction.CreatedAt
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transaction {TransactionId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // POST /Api/transaction/balance
    [HttpPost("balance")]
    public async Task<ActionResult<BalanceResponseDto>> GetBalance([FromBody] long telegramId)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var balance = await _transactionService.GetBalanceAsync(user.Id);
            var (income, expense) = await _transactionService.GetTotalIncomeExpenseAsync(user.Id);

            var response = new BalanceResponseDto
            {
                TelegramId = user.TelegramId,
                Username = user.Username,
                Balance = balance,
                TotalIncome = income,
                TotalExpense = expense
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for TelegramId {TelegramId}", telegramId);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    // POST /Api/transaction/recap
    [HttpPost("recap")]
    public async Task<ActionResult<RecapResponseDto>> GetRecap([FromBody] RecapRequestDto dto)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(dto.TelegramId);
            if (user == null)
            {
                return NotFound(new { error = "User not found" });
            }

            var (income, expense, balance) = await _transactionService.GetRecapAsync(user.Id, dto.StartDate, dto.EndDate);
            var transactions = await _transactionService.GetTransactionsByPeriodAsync(user.Id, dto.StartDate, dto.EndDate);

            var response = new RecapResponseDto
            {
                TotalIncome = income,
                TotalExpense = expense,
                Balance = balance,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                Transactions = transactions.Select(t => new TransactionResponseDto
                {
                    Id = t.Id,
                    Username = t.User.Username,
                    CategoryName = t.Category.Name,
                    Type = t.Type,
                    Amount = t.Amount,
                    Note = t.Note,
                    Date = t.Date,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recap");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}