using FluentAssertions;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Xunit;

namespace OrdersService.Domain.UnitTests.Entities;

public class OrderTests
{
    private static Order CreateNewOrder()
    {
        return new Order(
            Guid.NewGuid(),
            Guid.NewGuid(),
            100m,
            "Test Description");
    }

    [Fact]
    public void MarkAsFinished_WhenOrderIsNew_ShouldChangeStatusToFinished()
    {
        // Arrange
        var order = CreateNewOrder();

        // Act
        order.MarkAsFinished();

        // Assert
        order.Status.Should().Be(StatusEnum.Finished);
    }

    [Fact]
    public void MarkAsFinished_WhenOrderIsCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = CreateNewOrder();
        order.MarkAsCancelled();

        // Act
        Action act = () => order.MarkAsFinished();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot finish a cancelled order!");
    }

    [Fact]
    public void MarkAsCancelled_WhenOrderIsNew_ShouldChangeStatusToCancelled()
    {
        // Arrange
        var order = CreateNewOrder();

        // Act
        order.MarkAsCancelled();

        // Assert
        order.Status.Should().Be(StatusEnum.Cancelled);
    }

    [Fact]
    public void MarkAsCancelled_WhenOrderIsFinished_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = CreateNewOrder();
        order.MarkAsFinished();

        // Act
        Action act = () => order.MarkAsCancelled();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot cancel a finished order!");
    }

    [Fact]
    public void MarkAsCancelled_WhenOrderIsAlreadyCancelled_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var order = CreateNewOrder();
        order.MarkAsCancelled();

        // Act
        Action act = () => order.MarkAsCancelled();

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Order is already cancelled!");
    }
}