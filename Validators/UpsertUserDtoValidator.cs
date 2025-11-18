using FluentValidation;
using FinanceTracker.Api.DTOs;

namespace FinanceTracker.Api.Validators;

public class UpsertUserDtoValidator : AbstractValidator<UpsertUserDto>
{
    public UpsertUserDtoValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("TelegramId must be greater than 0");

        RuleFor(x => x.Username)
            .NotEmpty()
            .WithMessage("Username is required")
            .MaximumLength(100)
            .WithMessage("Username cannot exceed 100 characters");
    }
}