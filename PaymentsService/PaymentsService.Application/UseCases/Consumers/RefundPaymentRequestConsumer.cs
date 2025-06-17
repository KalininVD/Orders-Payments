using MassTransit;
using Microsoft.Extensions.Logging;
using PaymentsService.Application.Abstractions;
using Shared.Contracts.OrderEvents;

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

        var account = await _accountRepository.GetByUserIdAsync(message.UserId, context.CancellationToken);

        if (account is null)
        {
            _logger.LogError("Cannot refund UserId: {UserId}. Account not found.", message.UserId);
            return;
        }

        account.Deposit(message.Amount);

        await _unitOfWork.SaveChangesAsync(context.CancellationToken);

        _logger.LogInformation("Refund for UserId: {UserId} succeeded. Amount: {Amount}", message.UserId, message.Amount);
    }
}