using FluentValidation;

namespace OrdersService.Application.UseCases.CreateOrder;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.Amount)
            .GreaterThan(0)
            .WithMessage("Order amount must be positive.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .MaximumLength(256);
    }
}