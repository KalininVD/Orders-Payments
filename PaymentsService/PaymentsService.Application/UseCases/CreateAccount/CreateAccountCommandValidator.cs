using FluentValidation;

namespace PaymentsService.Application.UseCases.CreateAccount;

public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
{
    public CreateAccountCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}