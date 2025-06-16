using FluentAssertions;
using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.UseCases.DepositFunds;
using PaymentsService.Domain.Entities;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class DepositFundsCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DepositFundsCommandHandler _handler;

    public DepositFundsCommandHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new DepositFundsCommandHandler(
            _accountRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_DepositFundsAndSaveChanges_WhenAccountExists()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), 100m);
        var account = new Account(Guid.NewGuid(), command.UserId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        account.Balance.Should().Be(100m);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowKeyNotFoundException_WhenAccountDoesNotExist()
    {
        // Arrange
        var command = new DepositFundsCommand(Guid.NewGuid(), 100m);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"Account for user with ID {command.UserId} not found.");

        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}