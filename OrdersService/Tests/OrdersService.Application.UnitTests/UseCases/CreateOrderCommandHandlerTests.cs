using FluentAssertions;
using MassTransit;
using Moq;
using OrdersService.Application.Abstractions;
using OrdersService.Application.UseCases.CreateOrder;
using OrdersService.Domain.Entities;
using Shared.Contracts.OrderEvents;
using Xunit;

namespace OrdersService.Application.UnitTests.UseCases;

public class CreateOrderCommandHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly CreateOrderCommandHandler _handler;

    public CreateOrderCommandHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();

        _handler = new CreateOrderCommandHandler(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object);
    }

    [Fact]
    public async Task Handle_Should_AddOrderToRepositoryAndPublishEvent_WhenCalled()
    {
        // Arrange
        var command = new CreateOrderCommand(Guid.NewGuid(), 150.5m, "A valid order");
        Order? createdOrder = null;

        _orderRepositoryMock
            .Setup(repo => repo.Add(It.IsAny<Order>()))
            .Callback<Order>(order => createdOrder = order);

        // Act
        var resultOrderId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _orderRepositoryMock.Verify(repo => repo.Add(It.IsAny<Order>()), Times.Once);

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<OrderPaymentRequest>(e =>
                e.OrderId == resultOrderId &&
                e.UserId == command.UserId &&
                e.Amount == command.Amount),
            It.IsAny<CancellationToken>()), Times.Once);

        resultOrderId.Should().Be(createdOrder!.Id);
    }
}