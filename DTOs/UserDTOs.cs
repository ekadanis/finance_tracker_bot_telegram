using System.ComponentModel.DataAnnotations;

namespace FinanceTracker.Api.DTOs;

public class UserResponseDto
{
    public Guid Id { get; set; }
    public long TelegramId { get; set; }
    public string Username { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpsertUserDto
{
    [Required]
    public long TelegramId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Username { get; set; } = string.Empty;
}