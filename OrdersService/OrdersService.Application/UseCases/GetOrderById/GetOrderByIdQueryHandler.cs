using MediatR;
using OrdersService.Application.Abstractions;

namespace OrdersService.Application.UseCases.GetOrderById;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository) : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    public async Task<OrderResponse?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.Id, cancellationToken);

        if (order is null)
        {
            return null;
        }

        return new OrderResponse(order.Id, order.UserId, order.Amount, order.Description, order.Status.ToString());
    }
}