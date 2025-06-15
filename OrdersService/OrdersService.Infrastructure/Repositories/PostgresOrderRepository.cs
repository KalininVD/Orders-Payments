using Microsoft.EntityFrameworkCore;
using OrdersService.Application.Abstractions;
using OrdersService.Domain.Entities;

namespace OrdersService.Infrastructure.Repositories;

public class PostgresOrderRepository(OrdersDbContext context) : IOrderRepository
{
    private readonly OrdersDbContext _context = context;

    public void Add(Order order)
    {
        _context.Orders.Add(order);
    }

    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return await _context.Orders
            .Where(o => o.UserId == userId)
            .ToListAsync(cancellationToken);
    }
}