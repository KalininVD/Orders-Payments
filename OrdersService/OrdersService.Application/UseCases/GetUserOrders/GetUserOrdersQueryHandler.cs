using MediatR;
using OrdersService.Application.Abstractions;

namespace OrdersService.Application.UseCases.GetUserOrders;

public class GetUserOrdersQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetUserOrdersQuery, IEnumerable<OrdersResponse>>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<IEnumerable<OrdersResponse>> Handle(GetUserOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _orderRepository.GetByUserIdAsync(request.UserId, cancellationToken);

        var response = orders.Select(
            order => new OrdersResponse(order.Id, order.UserId, order.Amount, order.Description, order.Status.ToString())
        );

        return response;
    }
}