using System.Net.Http.Json;
using OrdersService.Application.Abstractions;
using Shared.Contracts.Notifications;
using Microsoft.Extensions.Logging;

namespace OrdersService.Infrastructure.Notifications;

public class ApiGatewayNotifier(IHttpClientFactory httpClientFactory, string baseUrl, string notifyEndpoint, ILogger<ApiGatewayNotifier> logger) : IOrderNotifier
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly string _baseUrl = baseUrl;
    private readonly string _notifyEndpoint = notifyEndpoint;
    private readonly ILogger<ApiGatewayNotifier> _logger = logger;

    public async Task NotifyOrderStatusChanged(Guid userId, Guid orderId, string newStatus)
    {
        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            var notifyUri = new Uri(new Uri(_baseUrl), _notifyEndpoint);

            var notification = new NotificationRequest(
                userId,
                $"Status of your order #{orderId} has been changed to `{newStatus}`");

            await httpClient.PostAsJsonAsync(notifyUri, notification);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send notification");
        }
    }
}