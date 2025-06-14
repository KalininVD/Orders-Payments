using OrdersService.Domain.Entities;

namespace OrdersService.Application.Abstractions;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IEnumerable<Order>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    void Add(Order order);
}