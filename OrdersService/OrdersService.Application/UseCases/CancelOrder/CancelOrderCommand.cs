using MediatR;

namespace OrdersService.Application.UseCases.CancelOrder;

public record CancelOrderCommand(Guid OrderId) : IRequest;