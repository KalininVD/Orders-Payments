using MassTransit;
using OrdersService.Application.Abstractions;

namespace OrdersService.Infrastructure.MessageBroker;

public class EventBus(IPublishEndpoint publishEndpoint) : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint = publishEndpoint;

    public Task PublishAsync<T>(T @event, CancellationToken cancellationToken) where T : class
    {
        return _publishEndpoint.Publish(@event, cancellationToken);
    }
}