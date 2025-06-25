namespace OrdersService.Application.Abstractions;

public interface IOrderNotifier
{
    Task NotifyOrderStatusChanged(Guid userId, Guid orderId, string newStatus);
}