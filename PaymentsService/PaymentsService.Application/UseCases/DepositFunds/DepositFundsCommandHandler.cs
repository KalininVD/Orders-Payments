using MediatR;
using PaymentsService.Application.Abstractions;

namespace PaymentsService.Application.UseCases.DepositFunds;

public class DepositFundsCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork) : IRequestHandler<DepositFundsCommand>
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(DepositFundsCommand request, CancellationToken cancellationToken)
    {
        var account = await _accountRepository.GetByUserIdAsync(request.UserId, cancellationToken)
            ?? throw new KeyNotFoundException($"Account for user with ID {request.UserId} not found.");

        account.Deposit(request.Amount);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}