using MediatR;
using OrdersService.Application.Abstractions;

namespace OrdersService.Application.UseCases.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IOrderNotifier notifier) : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly IOrderNotifier _notifier = notifier;

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");

        order.MarkAsCancelled();

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _notifier.NotifyOrderStatusChanged(order.UserId, order.Id, "CANCELLED");
    }
}