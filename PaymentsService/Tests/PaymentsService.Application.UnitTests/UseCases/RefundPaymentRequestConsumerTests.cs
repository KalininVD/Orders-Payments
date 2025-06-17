using FluentAssertions;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.UseCases.Consumers;
using PaymentsService.Domain.Entities;
using Shared.Contracts.OrderEvents;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class RefundPaymentRequestConsumerTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<RefundPaymentRequestConsumer>> _loggerMock;
    private readonly RefundPaymentRequestConsumer _consumer;

    public RefundPaymentRequestConsumerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<RefundPaymentRequestConsumer>>();

        _consumer = new RefundPaymentRequestConsumer(
            _accountRepositoryMock.Object,
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
    public async Task Consume_Should_DepositFunds_WhenAccountExists()
    {
        // Arrange
        var message = new RefundPaymentRequest(Guid.NewGuid(), 100m);
        var account = new Account(Guid.NewGuid(), message.UserId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        account.Balance.Should().Be(100m);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Consume_Should_DoNothing_WhenAccountDoesNotExist()
    {
        // Arrange
        var message = new RefundPaymentRequest(Guid.NewGuid(), 100m);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        var consumeContextMock = CreateConsumeContextMock(message);

        // Act
        await _consumer.Consume(consumeContextMock.Object);

        // Assert
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Consume_Should_RetryIndefinitely_OnConcurrencyException()
    {
        // Arrange
        var message = new RefundPaymentRequest(Guid.NewGuid(), 100m);
        var account = new Account(Guid.NewGuid(), message.UserId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(message.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        using var cts = new CancellationTokenSource();

        var consumeContextMock = CreateConsumeContextMock(message);
        consumeContextMock.Setup(ctx => ctx.Message).Returns(message);
        consumeContextMock.Setup(ctx => ctx.CancellationToken).Returns(cts.Token);

        _unitOfWorkMock
            .Setup(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new DbUpdateConcurrencyException());

        // Act
        var consumeTask = _consumer.Consume(consumeContextMock.Object);
        cts.CancelAfter(TimeSpan.FromMilliseconds(200));
        await Assert.ThrowsAsync<TaskCanceledException>(() => consumeTask);

        // Assert
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.AtLeast(2));
    }
}