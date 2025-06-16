namespace PaymentsService.Application.Abstractions;

public interface IDelayProvider
{
    Task Delay(TimeSpan duration, CancellationToken cancellationToken);
}