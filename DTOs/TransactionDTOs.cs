using System.ComponentModel.DataAnnotations;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.DTOs;

public class CreateTransactionDto
{
    public long TelegramId { get; set; }
    public string? Username { get; set; } 
    public string CategoryName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string? Note { get; set; }
    public DateOnly? Date { get; set; }
}

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecapRequestDto
{
    [Required]
    public long TelegramId { get; set; }

    [Required]
    public DateOnly StartDate { get; set; }

    [Required]
    public DateOnly EndDate { get; set; }
}

public class RecapResponseDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public List<TransactionResponseDto> Transactions { get; set; } = new();
}

public class BalanceResponseDto
{
    public long TelegramId { get; set; }
    public decimal Balance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
}