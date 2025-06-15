using FluentValidation;

namespace PaymentsService.Application.UseCases.DepositFunds;

public class DepositFundsCommandValidator : AbstractValidator<DepositFundsCommand>
{
    public DepositFundsCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Deposit amount must be positive.");
    }
}