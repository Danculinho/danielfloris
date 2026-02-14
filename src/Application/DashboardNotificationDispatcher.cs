using System.Text.Json;
using Microsoft.Extensions.Logging;
using OperationsService.Domain;
using OperationsService.Infrastructure;

namespace OperationsService.Application;

public sealed class DashboardNotificationDispatcher(ILogger<DashboardNotificationDispatcher> logger) : IInternalEventDispatcher
{
    public Task DispatchAsync(OutboxMessage message, CancellationToken cancellationToken)
    {
        // Internal notification channel + dashboard refresh trigger without external side effects.
        switch (message.EventType)
        {
            case nameof(JobStatusChanged):
                var job = JsonSerializer.Deserialize<JobStatusChanged>(message.Payload);
                logger.LogInformation("[dashboard-refresh] job {JobId} status {From}->{To}", job?.JobId, job?.PreviousStatus, job?.CurrentStatus);
                break;
            case nameof(OperationStarted):
                var started = JsonSerializer.Deserialize<OperationStarted>(message.Payload);
                logger.LogInformation("[internal-notify] operation {OperationId} started for job {JobId}", started?.OperationId, started?.JobId);
                break;
            case nameof(OperationFinished):
                var finished = JsonSerializer.Deserialize<OperationFinished>(message.Payload);
                logger.LogInformation("[dashboard-refresh] operation {OperationId} finished success={Success}", finished?.OperationId, finished?.Successful);
                break;
            case nameof(InventoryDeducted):
                var inventory = JsonSerializer.Deserialize<InventoryDeducted>(message.Payload);
                logger.LogInformation("[internal-notify] inventory deducted product={ProductId} qty={Qty}", inventory?.ProductId, inventory?.Quantity);
                break;
            default:
                logger.LogInformation("Unhandled outbox event type {EventType}", message.EventType);
                break;
        }

        return Task.CompletedTask;
    }
}
