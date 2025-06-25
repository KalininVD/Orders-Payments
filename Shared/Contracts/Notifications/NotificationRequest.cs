namespace Shared.Contracts.Notifications;

public record NotificationRequest(Guid UserId, string Message);