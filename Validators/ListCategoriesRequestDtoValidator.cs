using FluentValidation;
using FinanceTracker.Api.DTOs;

namespace FinanceTracker.Api.Validators;

public class ListCategoriesRequestDtoValidator : AbstractValidator<ListCategoriesRequestDto>
{
    public ListCategoriesRequestDtoValidator()
    {
        RuleFor(x => x.TelegramId)
            .GreaterThan(0)
            .WithMessage("TelegramId must be greater than 0");

        RuleFor(x => x.Type)
            .IsInEnum()
            .When(x => x.Type.HasValue)
            .WithMessage("Invalid transaction type");
    }
}