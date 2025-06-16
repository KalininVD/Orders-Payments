using FluentAssertions;
using Moq;
using OrdersService.Application.Abstractions;
using OrdersService.Application.UseCases.GetUserOrders;
using OrdersService.Domain.Entities;
using Xunit;

namespace OrdersService.Application.UnitTests.UseCases;

public class GetUserOrdersQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetUserOrdersQueryHandler _handler;

    public GetUserOrdersQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetUserOrdersQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnListOfOrderResponses_WhenOrdersExistForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        var orders = new List<Order>
        {
            new(Guid.NewGuid(), userId, 50m, "Order 1"),
            new(Guid.NewGuid(), userId, 75m, "Order 2")
        };

        _orderRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(orders);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);

        result.Should().BeEquivalentTo(orders.Select(o =>
            new OrdersResponse(o.Id, o.UserId, o.Amount, o.Description, o.Status.ToString())));
    }

    [Fact]
    public async Task Handle_Should_ReturnEmptyList_WhenNoOrdersExistForUser()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var query = new GetUserOrdersQuery(userId);

        _orderRepositoryMock
            .Setup(repo => repo.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }
}