using FluentAssertions;
using Moq;
using PaymentsService.Application.Abstractions;
using PaymentsService.Application.UseCases.GetAccountById;
using PaymentsService.Domain.Entities;
using Xunit;

namespace PaymentsService.Application.UnitTests.UseCases;

public class GetAccountByIdQueryHandlerTests
{
    private readonly Mock<IAccountRepository> _accountRepositoryMock;
    private readonly GetAccountByIdQueryHandler _handler;

    public GetAccountByIdQueryHandlerTests()
    {
        _accountRepositoryMock = new Mock<IAccountRepository>();
        _handler = new GetAccountByIdQueryHandler(_accountRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnAccountResponse_WhenAccountExists()
    {
        // Arrange
        var account = new Account(Guid.NewGuid(), Guid.NewGuid());
        var query = new GetAccountByIdQuery(account.Id);

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(account);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(new AccountResponse(account.Id, account.UserId, account.Balance));
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenAccountDoesNotExist()
    {
        // Arrange
        var query = new GetAccountByIdQuery(Guid.NewGuid());

        _accountRepositoryMock
            .Setup(repo => repo.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Account?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}