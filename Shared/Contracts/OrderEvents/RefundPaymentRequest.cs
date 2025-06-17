namespace Shared.Contracts.OrderEvents;

public record RefundPaymentRequest(Guid UserId, decimal Amount);