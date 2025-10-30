using System.ComponentModel.DataAnnotations;
using FinanceTracker.Api.Models;

namespace FinanceTracker.Api.DTOs;

public class CategoryResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public TransactionType Type { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class ListCategoriesRequestDto
{
    [Required]
    public long TelegramId { get; set; }

    public TransactionType? Type { get; set; }
}