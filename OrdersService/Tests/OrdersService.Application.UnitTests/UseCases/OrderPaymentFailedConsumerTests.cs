using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using OrdersService.Application.Abstractions;
using OrdersService.Application.UseCases.Consumers;
using OrdersService.Domain.Entities;
using OrdersService.Domain.Enums;
using Shared.Contracts.OrderEvents;
using Xunit;

namespace OrdersService.Application.UnitTests.UseCases;

public class OrderPaymentFailedConsumerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<OrderPaymentFailedConsumer>> _loggerMock;
    private readonly OrderPaymentFailedConsumer _consumer;

    public OrderPaymentFailedConsumerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<OrderPaymentFailedConsumer>>();

        _consumer = new OrderPaymentFailedConsumer(
            _orderRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _loggerMock.Object);
    }

    private static Mock<ConsumeContext<T>> CreateConsumeContextMock<T>(T message) where T : class
    {
        var mock = new Mock<ConsumeContext<T>>();
        mock.Setup(ctx => ctx.Message).Returns(message);
        return mock;
    }

    [Fact]
    public async Task Consume_Should_MarkOrderAsCancelled_WhenOrderExists()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), 100m, "Test Order");
        var message = new OrderPaymentFailed(order.Id, "Insufficient funds");

        order.Status.Should().Be(StatusEnum.New);

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(message.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        order.Status.Should().Be(StatusEnum.Cancelled);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_DoNothing_WhenOrderDoesNotExist()
    {
        // Arrange
        var message = new OrderPaymentFailed(Guid.NewGuid(), "Account not found");

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(message.OrderId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}