using Microsoft.EntityFrameworkCore;
using OperationsService.Infrastructure;

namespace OperationsService.Application;

public sealed class OutboxProcessorWorker(
    IServiceScopeFactory scopeFactory,
    ILogger<OutboxProcessorWorker> logger) : BackgroundService
{
    private const int MaxRetries = 5;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessBatchAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Outbox worker loop failed unexpectedly.");
            }

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);
        }
    }

    private async Task ProcessBatchAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var dispatcher = scope.ServiceProvider.GetRequiredService<IInternalEventDispatcher>();

        var now = DateTime.UtcNow;
        var messages = await db.Outbox
            .Where(x => !x.DeadLettered && x.ProcessedUtc == null && x.NextAttemptUtc <= now)
            .OrderBy(x => x.OccurredUtc)
            .Take(20)
            .ToListAsync(cancellationToken);

        if (messages.Count == 0)
        {
            return;
        }

        foreach (var message in messages)
        {
            try
            {
                await dispatcher.DispatchAsync(message, cancellationToken);
                message.ProcessedUtc = DateTime.UtcNow;
                message.LastError = null;
            }
            catch (Exception ex)
            {
                message.RetryCount += 1;
                message.LastError = ex.ToString();

                if (message.RetryCount >= MaxRetries)
                {
                    message.DeadLettered = true;
                    logger.LogError(ex, "Outbox message {MessageId} moved to dead-letter.", message.Id);
                }
                else
                {
                    var delaySeconds = Math.Pow(2, message.RetryCount);
                    message.NextAttemptUtc = DateTime.UtcNow.AddSeconds(delaySeconds);
                    logger.LogWarning(ex, "Outbox message {MessageId} failed. retry={RetryCount}", message.Id, message.RetryCount);
                }
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}
