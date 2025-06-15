using MassTransit;
using Microsoft.Extensions.Logging;
using OrdersService.Application.Abstractions;
using Shared.Contracts.OrderEvents;

namespace OrdersService.Application.UseCases.Consumers;

public class OrderPaymentSucceededConsumer(IOrderRepository orderRepository, IUnitOfWork unitOfWork, ILogger<OrderPaymentSucceededConsumer> logger) : IConsumer<OrderPaymentSucceeded>
{
    private readonly IOrderRepository _orderRepository = orderRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ILogger<OrderPaymentSucceededConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderPaymentSucceeded> context)
    {
        var message = context.Message;

        _logger.LogInformation("Received payment success event for OrderId: {OrderId}", message.OrderId);

        var order = await _orderRepository.GetByIdAsync(message.OrderId, context.CancellationToken);

        if (order is null)
        {
            _logger.LogWarning("Order with Id: {OrderId} not found.", message.OrderId);

            return;
        }

        order.MarkAsFinished();

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Order {OrderId} status updated to Finished.", order.Id);
    }
}