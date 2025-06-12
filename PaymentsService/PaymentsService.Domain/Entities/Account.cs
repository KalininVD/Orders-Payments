namespace PaymentsService.Domain.Entities;

public class Account
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public decimal Balance { get; private set; }

    public void Deposit(decimal amount)
    {
        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        Balance -= amount;
    }
}