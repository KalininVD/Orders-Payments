using MediatR;

namespace PaymentsService.Application.UseCases.CreateAccount;

public record CreateAccountCommand(Guid UserId) : IRequest<Guid>;