using FluentAssertions;
using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.UseCases.CreateAccount;
using PaymentsService.Domain.Entities;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class CreateAccountCommandHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateAccountCommandHandler _handler;

    public CreateAccountCommandHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateAccountCommandHandler(
            _accountRepositoryMock.Object,
            _unitOfWorkMock.Object);
    }

    [Fact]
    public async Task Handle_Should_CreateAndAddAccount_WhenUserDoesNotExist()
    {
        // Arrange
        var command = new CreateAccountCommand(Guid.NewGuid());

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var accountId = await _handler.Handle(command, CancellationToken.None);

        // Assert
        accountId.Should().NotBeEmpty();

        _accountRepositoryMock.Verify(repo => repo.Add(It.IsAny<Account>()), Times.Once);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_ThrowInvalidOperationException_WhenUserAlreadyExists()
    {
        // Arrange
        var command = new CreateAccountCommand(Guid.NewGuid());
        var existingAccount = new Account(Guid.NewGuid(), command.UserId);

        _accountRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(command.UserId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAccount);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"An account for user with ID {command.UserId} already exists!");

        _accountRepositoryMock.Verify(repo => repo.Add(It.IsAny<Account>()), Times.Never);
        _unitOfWorkMock.Verify(uow => uow.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
}