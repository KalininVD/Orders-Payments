using MediatR;
using PaymentsService.Application.Abstractions;

namespace PaymentsService.Application.UseCases.GetAccountById;

public class GetAccountByIdQueryHandler(IAccountRepository accountRepository) : IRequestHandler<GetAccountByIdQuery, AccountResponse?>
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    public async Task<AccountResponse?> Handle(GetAccountByIdQuery request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByIdAsync(request.Id, cancellationToken);

        if (account is null)
        {
            return null;
        }

        return new AccountResponse(account.Id, account.UserId, account.Balance);
    }
}