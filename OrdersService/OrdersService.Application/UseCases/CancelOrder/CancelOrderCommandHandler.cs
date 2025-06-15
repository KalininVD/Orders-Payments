using MediatR;
using OrdersService.Application.Abstractions;

namespace OrdersService.Application.UseCases.CancelOrder;

public class CancelOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork) : IRequestHandler<CancelOrderCommand>
{
    private readonly IOrderRepository _orderRepository = orderRepository;

    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    public async Task Handle(CancelOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _orderRepository.GetByIdAsync(request.OrderId, cancellationToken)
            ?? throw new KeyNotFoundException($"Order with id {request.OrderId} not found.");

        order.MarkAsCancelled();

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}