using MediatR;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.UseCases.GetAccountById;

namespace PaymentsService.Application.UseCases.GetAccountByUserId;

public class GetAccountByUserIdQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAccountByUserIdQuery, AccountResponse?>
{
    public async Task<AccountResponse?> Handle(GetAccountByUserIdQuery request, CancellationToken cancellationToken)
    {
        var account = await accountRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (account is null)
        {
            return null;
        }

        return new AccountResponse(account.Id, account.UserId, account.Balance);
    }
}