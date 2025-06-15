using MassTransit;
using Microsoft.Extensions.Logging;
using OrdersService.Application.Abstractions;
using Shared.Contracts.OrderEvents;

namespace OrdersService.Application.UseCases.Consumers;

public class OrderPaymentFailedConsumer(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<OrderPaymentFailedConsumer> logger) : IConsumer<OrderPaymentFailed>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ILogger<OrderPaymentFailedConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderPaymentFailed> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received payment failed event for OrderId: {OrderId}", message.OrderId);

        var order = await _orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order with Id: {OrderId} not found.", message.OrderId);

            return;
        }

        order.MarkAsCancelled();

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to Cancelled.", order.Id);
    }
}