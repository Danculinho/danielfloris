using OperationsService.Infrastructure;

namespace OperationsService.Application;

public interface IInternalEventDispatcher
{
    Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken);
}
