namespace Shared.Contracts.OrderEvents;

public record OrderPaymentRequest(Guid OrderId, Guid UserId, decimal Amount);