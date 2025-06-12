using OrdersService.Domain.ValueObjects;

namespace OrdersService.Domain.Entities;

public class Order
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public decimal Amount { get; init; }

    public string Description { get; init; } = string.Empty;

    public StatusVO Status { get; init; } = StatusVO.New;
}