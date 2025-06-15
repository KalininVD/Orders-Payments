using Microsoft.EntityFrameworkCore;
using PaymentsService.Application.Abstractions;
using PaymentsService.Domain.Entities;

namespace PaymentsService.Infrastructure.Repositories;

public class PostgresAccountRepository(PaymentsDbContext context) : IAccountRepository
{
    private readonly PaymentsDbContext _context = context;

    public void Add(Account account)
    {
        _context.Accounts.Add(account);
    }

    public async Task<Account?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);
    }
}