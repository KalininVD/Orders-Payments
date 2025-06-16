using FluentAssertions;
using MassTransit;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.UseCases.Consumers;
using PaymentsService.Domain.Entities;
using Shared.Contracts.OrderEvents;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class OrderPaymentRequestConsumerTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IPublishEndpoint> _publishEndpointMock;
    private readonly Mock<IDelayProvider> _delayProviderMock;
    private readonly Mock<ILogger<OrderPaymentRequestConsumer>> _loggerMock;
    private readonly OrderPaymentRequestConsumer _consumer;

    public OrderPaymentRequestConsumerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _publishEndpointMock = new Mock<IPublishEndpoint>();
        _delayProviderMock = new Mock<IDelayProvider>();
        _loggerMock = new Mock<ILogger<OrderPaymentRequestConsumer>>();

        _consumer = new OrderPaymentRequestConsumer(
            _accountRepositoryMock.Object,
            _unitOfWorkMock.Object,
            _publishEndpointMock.Object,
            _delayProviderMock.Object,
            _loggerMock.Object);
    }

    private static Mock<ConsumeContext<T>> CreateConsumeContextMock<T>(T message) where T : class
    {
        var mock = new Mock<ConsumeContext<T>>();
        mock.Setup(ctx => ctx.Message).Returns(message);
        return mock;
    }

    [Fact]
    public async Task Consume_Should_WithdrawAndPublishSucceededEvent_WhenAccountHasSufficientFunds()
    {
        // Arrange
        var message = new OrderPaymentRequest(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var account = new Account(Guid.NewGuid(), message.UserId);
        account.Deposit(150m);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        account.Balance.Should().Be(50m);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<OrderPaymentSucceeded>(e => e.OrderId == message.OrderId),
            It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderPaymentFailed>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_Should_PublishFailedEvent_WhenAccountHasInsufficientFunds()
    {
        // Arrange
        var message = new OrderPaymentRequest(Guid.NewGuid(), Guid.NewGuid(), 100m);
        var account = new Account(Guid.NewGuid(), message.UserId);
        account.Deposit(50m);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        account.Balance.Should().Be(50m);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<OrderPaymentFailed>(e => e.OrderId == message.OrderId && e.Reason.Contains("Insufficient funds")),
            It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderPaymentSucceeded>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_Should_PublishFailedEvent_WhenAccountNotFound()
    {
        // Arrange
        var message = new OrderPaymentRequest(Guid.NewGuid(), Guid.NewGuid(), 100m);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);

        _publishEndpointMock.Verify(p => p.Publish(
            It.Is<OrderPaymentFailed>(e => e.OrderId == message.OrderId && e.Reason == "Account not found."),
            It.IsAny<CancellationToken>()), Times.Once);

        _publishEndpointMock.Verify(p => p.Publish(It.IsAny<OrderPaymentSucceeded>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}