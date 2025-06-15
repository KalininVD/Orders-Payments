namespace Shared.Contracts.OrderEvents;

public record OrderPaymentFailed(Guid OrderId, string Reason);