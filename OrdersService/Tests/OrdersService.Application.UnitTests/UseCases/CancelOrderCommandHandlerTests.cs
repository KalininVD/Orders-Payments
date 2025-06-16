using FluentAssertions;
using Moq;
using OrdersService.Application.Abstractions;
using OrdersService.Application.UseCases.CancelOrder;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Xunit;

namespace OrdersService.Application.UnitTests.UseCases;

public class CancelOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CancelOrderCommandHandler _handler;

    public CancelOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CancelOrderCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_MarkOrderAsCancelled_WhenOrderExistsAndIsInNewState()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), 100m, "Test Order");
        var command = new CancelOrderCommand(order.Id);

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        order.Status.Should().Be(StatusEnum.Cancelled);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowKeyNotFoundException_WhenOrderDoesNotExist()
    {
        // Arrange
        var command = new CancelOrderCommand(Guid.NewGuid());

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Order with id {command.OrderId} not found.");

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenTryingToCancelFinishedOrder()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), 100m, "Finished Order");
        order.MarkAsFinished();

        var command = new CancelOrderCommand(order.Id);

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(command.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Cannot cancel a finished order!");

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}