using FluentValidation;
using FinanceTracker.Api.DTOs;

namespace FinanceTracker.Api.Validators;

public class CreateTransactionDtoValidator : AbstractValidator<CreateTransactionDto>
{
    public CreateTransactionDtoValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("TelegramId must be greater than 0");

        RuleFor(x => x.CategoryName)
            .NotEmpty()
            .WithMessage("Category name is required")
            .MaximumLength(100)
            .WithMessage("Category name cannot exceed 100 characters");

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Amount must be greater than 0")
            .LessThanOrEqualTo(999999999999.99m)
            .WithMessage("Amount is too large");

        RuleFor(x => x.Type)
            .IsInEnum()
            .WithMessage("Invalid transaction type");

        RuleFor(x => x.Note)
            .MaximumLength(500)
            .WithMessage("Note cannot exceed 500 characters");
    }
}