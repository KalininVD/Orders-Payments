using MediatR;

namespace PaymentsService.Application.UseCases.GetAccountById;

public record GetAccountByIdQuery(Guid Id) : IRequest<AccountResponse?>;