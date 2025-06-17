using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentsService.Application.Abstractions;
using Shared.Contracts.OrderEvents;
using Microsoft.EntityFrameworkCore;

namespace PaymentsService.Application.UseCases.Consumers;

public class OrderPaymentRequestConsumer(IAccountRepository accountRepository, IUnitOfWork unitOfWork, IPublishEndpoint publishEndpoint, IDelayProvider delayProvider, ILogger<OrderPaymentRequestConsumer> logger) : IConsumer<OrderPaymentRequest>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;
    private readonly IDelayProvider _delayProvider = delayProvider;

    private readonly ILogger<OrderPaymentRequestConsumer> _logger = logger;

    private const int MaxRetries = 3;

    public async Task Consume(ConsumeContext<OrderPaymentRequest> context)
    {
        var message = context.Message;

        _logger.LogInformation(
            "Received payment request for OrderId: {OrderId}, UserId: {UserId}, Amount: {Amount}",
            message.OrderId, message.UserId, message.Amount);

        _logger.LogDebug("Simulating payment processing delay for OrderId: {OrderId}", message.OrderId);
        await _delayProvider.Delay(TimeSpan.FromSeconds(30), context.CancellationToken);

        for (int i = 0; i < MaxRetries; ++i)
        {
            try
            {
                var account = await _accountRepository.GetByUserIdAsync(message.UserId, context.CancellationToken);

                if (account is null)
                {
                    _logger.LogWarning("Account for UserId: {UserId} not found.", message.UserId);
                    await _publishEndpoint.Publish(new OrderPaymentFailed(message.OrderId, "Account not found."), context.CancellationToken);
                    return;
                }

                account.Withdraw(message.Amount);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation("Payment for OrderId: {OrderId} succeeded on attempt {Attempt}", message.OrderId, i + 1);

                await _publishEndpoint.Publish(new OrderPaymentSucceeded(message.OrderId), context.CancellationToken);
                break;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict for OrderId: {OrderId} on attempt {Attempt}. Retrying...", message.OrderId, i + 1);

                if (i == MaxRetries - 1)
                {
                    _logger.LogError(
                        "Payment for OrderId: {OrderId} failed after {Retries} retries due to concurrency.",
                        message.OrderId, MaxRetries
                    );

                    await _publishEndpoint.Publish(
                        new OrderPaymentFailed(message.OrderId, "Failed to process payment due to high contention."),
                        context.CancellationToken
                    );

                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(50 + Random.Shared.Next(0, 100)), context.CancellationToken);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Payment failed for OrderId: {OrderId}. Reason: {Reason}", message.OrderId, ex.Message);
                await _publishEndpoint.Publish(new OrderPaymentFailed(message.OrderId, ex.Message), context.CancellationToken);
                return;
            }
        }
    }
}