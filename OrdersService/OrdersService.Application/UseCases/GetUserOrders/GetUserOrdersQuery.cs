using MediatR;

namespace OrdersService.Application.UseCases.GetUserOrders;

public record GetUserOrdersQuery(Guid UserId) : IRequest<IEnumerable<OrdersResponse>>;