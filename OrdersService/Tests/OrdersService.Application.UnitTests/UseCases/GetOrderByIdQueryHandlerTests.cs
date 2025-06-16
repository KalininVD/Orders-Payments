using FluentAssertions;
using Moq;
using OrdersService.Application.Abstractions;
using OrdersService.Application.UseCases.GetOrderById;
using OrdersService.Domain.Entities;
using Xunit;

namespace OrdersService.Application.UnitTests.UseCases;

public class GetOrderByIdQueryHandlerTests
{
    private readonly Mock<IOrderRepository> _orderRepositoryMock;
    private readonly GetOrderByIdQueryHandler _handler;

    public GetOrderByIdQueryHandlerTests()
    {
        _orderRepositoryMock = new Mock<IOrderRepository>();
        _handler = new GetOrderByIdQueryHandler(_orderRepositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_ReturnOrderResponse_WhenOrderExists()
    {
        // Arrange
        var order = new Order(Guid.NewGuid(), Guid.NewGuid(), 100m, "Test Order");
        var query = new GetOrderByIdQuery(order.Id);

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(order.Id);
        result.Description.Should().Be(order.Description);
        result.Status.Should().Be(order.Status.ToString());
    }

    [Fact]
    public async Task Handle_Should_ReturnNull_WhenOrderDoesNotExist()
    {
        // Arrange
        var query = new GetOrderByIdQuery(Guid.NewGuid());

        _orderRepositoryMock
            .Setup(repo => repo.GetByIdAsync(query.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Order?)null);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }
}