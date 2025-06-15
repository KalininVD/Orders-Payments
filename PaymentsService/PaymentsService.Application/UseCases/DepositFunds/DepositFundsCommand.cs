using MediatR;

namespace PaymentsService.Application.UseCases.DepositFunds;

public record DepositFundsCommand(Guid UserId, decimal Amount) : IRequest;