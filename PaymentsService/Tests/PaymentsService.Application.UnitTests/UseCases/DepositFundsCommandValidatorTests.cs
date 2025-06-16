using FluentAssertions;
using PaymentsService.Application.UseCases.DepositFunds;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class DepositFundsCommandValidatorTests
{
    private readonly DepositFundsCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_UserIdIsEmpty()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.Empty, 100m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.UserId));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50.0)]
    public void Should_HaveError_When_AmountIsZeroOrNegative(decimal invalidAmount)
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), invalidAmount);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Amount));
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), 100m);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}