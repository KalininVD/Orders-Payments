using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentsService.Application.Abstractions;
using Shared.Contracts.OrderEvents;
using Microsoft.EntityFrameworkCore;

namespace PaymentsService.Application.UseCases.Consumers;

public class RefundPaymentRequestConsumer(IAccountRepository accountRepository, IUnitOfWork unitOfWork, ILogger<RefundPaymentRequestConsumer> logger) : IConsumer<RefundPaymentRequest>
{
    private readonly IAccountRepository _accountRepository = accountRepository;
    private readonly IUnitOfWork _unitOfWork = unitOfWork;

    private readonly ILogger<RefundPaymentRequestConsumer> _logger = logger;

    public async Task Consume(ConsumeContext<RefundPaymentRequest> context)
    {
        var message = context.Message;

        _logger.LogInformation("Processing refund request for UserId: {UserId}, Amount: {Amount}", message.UserId, message.Amount);

        while (true)
        {
            try
            {
                var account = await _accountRepository.GetByUserIdAsync(message.UserId, context.CancellationToken);

                if (account is null)
                {
                    _logger.LogError("Cannot refund UserId: {UserId}. Account not found.", message.UserId);
                    return;
                }

                account.Deposit(message.Amount);

                await _unitOfWork.SaveChangesAsync(context.CancellationToken);
                _logger.LogInformation("Refund for UserId: {UserId} succeeded.", message.UserId);

                break;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict for OrderId: {OrderId}. Retrying...", message.UserId);

                await Task.Delay(TimeSpan.FromMilliseconds(50 + Random.Shared.Next(0, 100)), context.CancellationToken);
            }
        }
    }
}