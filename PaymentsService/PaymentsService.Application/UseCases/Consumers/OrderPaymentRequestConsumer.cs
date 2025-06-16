using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentsService.Application.Abstractions;
using Shared.Contracts.OrderEvents;

namespace PaymentsService.Application.UseCases.Consumers;

public class OrderPaymentRequestConsumer(IAccountRepository accountRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, IDelayProvider delayProvider, ILogger<OrderPaymentRequestConsumer> logger) : IConsumer<OrderPaymentRequest>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IDelayProvider _delayProvider = delayProvider;

    private readonly ILogger<OrderPaymentRequestConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<OrderPaymentRequest> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received payment request for OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
            message.OrderId, message.UserId, message.Amount);

        _logger.LogDebug("Simulating payment processing delay for 30 seconds...");

        await _delayProvider.Delay(TimeSpan.FromSeconds(30), context.CancellationToken);

        var account = await _accountRepository.GetByUserIdAsync(message.UserId, context.CancellationToken);

        if (account is null)
        {
            _logger.LogWarning(
                "Account for UserId: {UserId} not found. Failing payment for OrderId: {OrderId}",
                message.UserId, message.OrderId);

            await _publishEndpoint.Publish(new OrderPaymentFailed(message.OrderId, "Account not found."), context.CancellationToken);

            return;
        }

        try
        {
            account.Withdraw(message.Amount);

            await _unitOfWork.SaveChangesAsync(context.CancellationToken);

            _logger.LogInformation(
                "Successfully processed payment for OrderId: {OrderId}",
                message.OrderId);

            await _publishEndpoint.Publish(new OrderPaymentSucceeded(message.OrderId), context.CancellationToken);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex,
                "Payment failed for OrderId: {OrderId}. Reason: Insufficient funds.",
                message.OrderId);

            await _publishEndpoint.Publish(new OrderPaymentFailed(message.OrderId, ex.Message), context.CancellationToken);
        }
    }
}