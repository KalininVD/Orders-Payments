using FluentAssertions;
using OrdersService.Application.UseCases.CreateOrder;
using Xunit;

namespace OrdersService.Application.UnitTests.UseCases;

public class CreateOrderCommandValidatorTests
{
    private readonly CreateOrderCommandValidator _validator = new();

    [Fact]
    public void Should_HaveError_When_UserIdIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.Empty, 100m, "Test Description");

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
        var command = new CreateOrderCommand(Guid.NewGuid(), invalidAmount, "Test Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Amount));
    }

    [Fact]
    public void Should_HaveError_When_DescriptionIsEmpty()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), 100m, string.Empty);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Description));
    }

    [Fact]
    public void Should_NotHaveError_When_CommandIsValid()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), 100m, "Valid Description");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}