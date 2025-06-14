using PaymentsService.Domain.Entities;

namespace PaymentsService.Application.Abstractions;

public interface IAccountRepository
{
    Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Account account);
}