using PaymentsService.Application.Abstractions;

namespace PaymentsService.Infrastructure.Services;

public class DefaultDelayProvider : IDelayProvider
{
    public Task Delay(TimeSpan duration, CancellationToken cancellationToken)
    {
        return Task.Delay(duration, cancellationToken);
    }
}