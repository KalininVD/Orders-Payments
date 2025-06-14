namespace PaymentsService.Domain.Entities;

public class Account
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public decimal Balance { get; private set; }

    private Account() { }

    public Account(Guid id, Guid userId)
    {
        Id = id;
        UserId = userId;
        Balance = 0;
    }

    public void Deposit(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Deposit amount must be positive!");
        }

        Balance += amount;
    }

    public void Withdraw(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Withdrawal amount must be positive!");
        }

        if (Balance < amount)
        {
            throw new InvalidOperationException($"Insufficient funds: account balance is {Balance}, but you want to withdraw {amount}.");
        }

        Balance -= amount;
    }
}