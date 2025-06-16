using FluentAssertions;
using PaymentsService.Domain.Entities;
using Xunit;

namespace PaymentsService.Domain.UnitTests.Entities;

public class AccountTests
{
    private static Account CreateTestAccount()
    {
        return new Account(Guid.NewGuid(), Guid.NewGuid());
    }

    [Fact]
    public void Constructor_WhenCalled_ShouldCreateAccountWithZeroBalance()
    {
        // Arrange & Act
        var account = CreateTestAccount();

        // Assert
        account.Balance.Should().Be(0);
        account.Id.Should().NotBeEmpty();
        account.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public void Deposit_WithPositiveAmount_ShouldIncreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount();
        var depositAmount = 100.50m;

        // Act
        account.Deposit(depositAmount);

        // Assert
        account.Balance.Should().Be(depositAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50.0)]
    public void Deposit_WithZeroOrNegativeAmount_ShouldThrowArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Arrange
        var account = CreateTestAccount();

        // Act
        Action act = () => account.Deposit(invalidAmount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("amount");
    }

    [Fact]
    public void Withdraw_WithSufficientFunds_ShouldDecreaseBalance()
    {
        // Arrange
        var account = CreateTestAccount();
        account.Deposit(200m);
        var withdrawalAmount = 75m;

        // Act
        account.Withdraw(withdrawalAmount);

        // Assert
        account.Balance.Should().Be(125m); // 200 - 75 = 125
    }

    [Fact]
    public void Withdraw_WithInsufficientFunds_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var account = CreateTestAccount();
        account.Deposit(50m);
        var withdrawalAmount = 100m;

        // Act
        Action act = () => account.Withdraw(withdrawalAmount);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage($"Insufficient funds: account balance is 50, but you want to withdraw 100.");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100.0)]
    public void Withdraw_WithZeroOrNegativeAmount_ShouldThrowArgumentOutOfRangeException(decimal invalidAmount)
    {
        // Arrange
        var account = CreateTestAccount();
        account.Deposit(200m);

        // Act
        Action act = () => account.Withdraw(invalidAmount);

        // Assert
        act.Should().Throw<ArgumentOutOfRangeException>()
            .And.ParamName.Should().Be("amount");
    }
}