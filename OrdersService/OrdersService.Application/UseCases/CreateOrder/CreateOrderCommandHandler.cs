using MediatR;
using MassTransit;
using OrdersService.Application.Abstractions;
using OrdersService.Domain.Entities;
using Shared.Contracts.OrderEvents;

namespace OrdersService.Application.UseCases.CreateOrder;

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint) : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public async Task<Guid> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order(Guid.NewGuid(), request.UserId, request.Amount, request.Description);

        _orderRepository.Add(order);

        var paymentRequest = new OrderPaymentRequest(order.Id, order.UserId, order.Amount);

        await _publishEndpoint.Publish(paymentRequest, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return order.Id;
    }
}