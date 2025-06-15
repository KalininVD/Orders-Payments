using OrdersService.Domain.Enums;

namespace OrdersService.Domain.Entities;

public class Order
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public decimal Amount { get; init; }

    public string Description { get; init; } = string.Empty;

    public StatusEnum Status { get; private set; }

    private Order() { }

    public Order(Guid id, Guid userId, decimal amount, string description)
    {
        Id = id;
        UserId = userId;
        Amount = amount;
        Description = description ?? string.Empty;
        Status = StatusEnum.New;
    }

    public void MarkAsFinished()
    {
        if (Status == StatusEnum.Cancelled)
        {
            throw new InvalidOperationException("Cannot finish a cancelled order!");
        }

        Status = StatusEnum.Finished;
    }

    public void MarkAsCancelled()
    {
        if (Status == StatusEnum.Finished)
        {
            throw new InvalidOperationException("Cannot cancel a finished order!");
        }

        Status = StatusEnum.Cancelled;
    }
}