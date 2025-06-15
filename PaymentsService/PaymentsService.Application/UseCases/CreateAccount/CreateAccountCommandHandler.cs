using MediatR;
using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.UseCases.CreateAccount;

public class CreateAccountCommandHandler(IAccountRepository accountRepository, IUnitOfWork unitOfWork) : IRequestHandler<CreateAccountCommand, Guid>
{
    private readonly IAccountRepository _accountRepository = accountRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task<Guid> Handle(CreateAccountCommand request, CancellationToken cancellationToken)
    {
        var existingAccount = await _accountRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        if (existingAccount is not null)
        {
            throw new InvalidOperationException($"An account for user with ID {request.UserId} already exists!");
        }

        var account = new Account(Guid.NewGuid(), request.UserId);

        _accountRepository.Add(account);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return account.Id;
    }
}