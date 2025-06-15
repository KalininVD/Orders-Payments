using MediatR;
using PaymentsService.Application.UseCases.GetAccountById;

namespace PaymentsService.Application.UseCases.GetAccountByUserId;

public record GetAccountByUserIdQuery(Guid UserId) : IRequest<AccountResponse?>;