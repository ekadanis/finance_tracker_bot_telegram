using Microsoft.AspNetCore.Mvc;
using FinanceTracker.Api.DTOs;
using FinanceTracker.Api.Interfaces;

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
    public async Task<ActionResult<ApiResponse<TransactionResponseDto>>> CreateTransaction([FromBody] CreateTransactionDto dto)
    {
        try
        {
            var username = dto.Username ?? $"User_{dto.TelegramId}";
            var user = await _userService.GetOrCreateUserAsync(dto.TelegramId, username);

            var category = await _categoryService.GetOrCreateCategoryAsync(user.Id, dto.CategoryName, dto.Type);
            if (category == null)
            {
                return BadRequest(ApiResponse<TransactionResponseDto>.ErrorResponse("Failed to create or retrieve category"));
            }

            var transactionDate = dto.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
            var transaction = await _transactionService.AddTransactionAsync(
                user.Id,
                category.Id,
                dto.Type,
                dto.Amount,
                dto.Note ?? string.Empty,
                transactionDate
            );

            var response = new TransactionResponseDto
            {
                Id = transaction.Id,
                CategoryName = category.Name,
                Type = transaction.Type,
                Amount = transaction.Amount,
                Note = transaction.Note,
                Date = transaction.Date,
                CreatedAt = transaction.CreatedAt
            };

            return Ok(ApiResponse<TransactionResponseDto>.SuccessResponse(response, "Transaction created successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating transaction");
            return StatusCode(500, ApiResponse<TransactionResponseDto>.ErrorResponse("Internal server error"));
        }
    }

    // GET /Api/transaction
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<TransactionResponseDto>>>> GetTransactions(
        [FromQuery] long telegramId,
        [FromQuery] string? startDate = null,
        [FromQuery] string? endDate = null)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return NotFound(ApiResponse<List<TransactionResponseDto>>.ErrorResponse("User not found"));
            }

            DateOnly? start = null;
            DateOnly? end = null;

            // Parse startDate if provided
            if (!string.IsNullOrWhiteSpace(startDate))
            {
                if (!DateOnly.TryParseExact(startDate, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out var parsedStart))
                {
                    return BadRequest(ApiResponse<List<TransactionResponseDto>>.ErrorResponse("Invalid startDate format. Use: YYYY-MM-DD"));
                }
                start = parsedStart;
            }

            // Parse endDate if provided
            if (!string.IsNullOrWhiteSpace(endDate))
            {
                if (!DateOnly.TryParseExact(endDate, "yyyy-MM-dd", 
                    System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out var parsedEnd))
                {
                    return BadRequest(ApiResponse<List<TransactionResponseDto>>.ErrorResponse("Invalid endDate format. Use: YYYY-MM-DD"));
                }
                end = parsedEnd;
            }

            // If no dates provided, use wide range to get all transactions
            var effectiveStart = start ?? new DateOnly(2000, 1, 1);
            var effectiveEnd = end ?? new DateOnly(2100, 12, 31);
            
            _logger.LogInformation("Fetching transactions for user {UserId} from {Start} to {End}", 
                user.Id, effectiveStart, effectiveEnd);
            
            var transactions = await _transactionService.GetTransactionsByPeriodAsync(user.Id, effectiveStart, effectiveEnd);

            var response = transactions.Select(t => new TransactionResponseDto
            {
                Id = t.Id,
                CategoryName = t.Category?.Name ?? "N/A",
                Type = t.Type,
                Amount = t.Amount,
                Note = t.Note ?? string.Empty,
                Date = t.Date,
                CreatedAt = t.CreatedAt
            }).ToList();

            return Ok(ApiResponse<List<TransactionResponseDto>>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting transactions");
            return StatusCode(500, ApiResponse<List<TransactionResponseDto>>.ErrorResponse("Internal server error"));
        }
    }

    // GET /Api/transaction/balance
    [HttpGet("balance")]
    public async Task<ActionResult<ApiResponse<BalanceResponseDto>>> GetBalance([FromQuery] long telegramId)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(telegramId);
            if (user == null)
            {
                return NotFound(ApiResponse<BalanceResponseDto>.ErrorResponse("User not found"));
            }

            var balance = await _transactionService.GetBalanceAsync(user.Id);
            var (income, expense) = await _transactionService.GetTotalIncomeExpenseAsync(user.Id);

            var response = new BalanceResponseDto
            {
                TelegramId = user.TelegramId,
                Balance = balance,
                TotalIncome = income,
                TotalExpense = expense
            };

            return Ok(ApiResponse<BalanceResponseDto>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting balance for TelegramId {TelegramId}", telegramId);
            return StatusCode(500, ApiResponse<BalanceResponseDto>.ErrorResponse("Internal server error"));
        }
    }

    // GET /Api/transaction/recap
    [HttpGet("recap")]
    public async Task<ActionResult<ApiResponse<RecapResponseDto>>> GetRecap([FromBody] RecapRequestDto dto)
    {
        try
        {
            var user = await _userService.GetUserByTelegramIdAsync(dto.TelegramId);
            if (user == null)
            {
                return NotFound(ApiResponse<RecapResponseDto>.ErrorResponse("User not found"));
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
                    CategoryName = t.Category.Name,
                    Type = t.Type,
                    Amount = t.Amount,
                    Note = t.Note,
                    Date = t.Date,
                    CreatedAt = t.CreatedAt
                }).ToList()
            };

            return Ok(ApiResponse<RecapResponseDto>.SuccessResponse(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting recap");
            return StatusCode(500, ApiResponse<RecapResponseDto>.ErrorResponse("Internal server error"));
        }
    }
}