using MediatR;

namespace OrdersService.Application.UseCases.GetOrderById;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderResponse?>;