using System.ComponentModel.DataAnnotations;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.DTOs;

public class CreateTransactionDto
{
    [Required]
    public long TelegramId { get; set; }

    [Required]
    [MaxLength(100)]
    public string CategoryName { get; set; } = string.Empty;

    [Required]
    public TransactionType Type { get; set; }

    [Required]
    [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
    public decimal Amount { get; set; }

    [MaxLength(500)]
    public string Note { get; set; } = string.Empty;

    public DateTime? Date { get; set; }
}

public class TransactionResponseDto
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string CategoryName { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public string Note { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RecapRequestDto
{
    [Required]
    public long TelegramId { get; set; }

    [Required]
    public DateTime StartDate { get; set; }

    [Required]
    public DateTime EndDate { get; set; }
}

public class RecapResponseDto
{
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
    public decimal Balance { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public List<TransactionResponseDto> Transactions { get; set; } = new();
}

public class BalanceResponseDto
{
    public long TelegramId { get; set; }
    public string Username { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    public decimal TotalIncome { get; set; }
    public decimal TotalExpense { get; set; }
}