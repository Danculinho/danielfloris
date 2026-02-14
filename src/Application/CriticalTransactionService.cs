using Microsoft.EntityFrameworkCore;
using OperationsService.Domain;
using OperationsService.Infrastructure;

namespace OperationsService.Application;

public sealed class CriticalTransactionService(AppDbContext dbContext)
{
    public async Task UpdateJobStatusAsync(Guid jobId, string newStatus, string changedBy, CancellationToken cancellationToken)
    {
        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);

        var job = await dbContext.Jobs.FirstOrDefaultAsync(x => x.Id == jobId, cancellationToken)
            ?? new Job { Id = jobId };

        var previousStatus = job.Status;
        job.Status = newStatus;
        job.UpdatedUtc = DateTime.UtcNow;

        dbContext.Update(job);

        var operation = new Operation
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            State = "Started",
            UpdatedUtc = DateTime.UtcNow
        };

        dbContext.Operations.Add(operation);

        var events = new IDomainEvent[]
        {
            new JobStatusChanged(jobId, previousStatus, newStatus, $"Changed by {changedBy}"),
            new OperationStarted(operation.Id, jobId, changedBy),
            new InventoryDeducted(Guid.NewGuid(), 1, operation.Id),
            new OperationFinished(operation.Id, jobId, true, "Operation completed successfully")
        };

        foreach (var domainEvent in events)
        {
            dbContext.Outbox.Add(OutboxMessage.FromEvent(domainEvent));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);
    }
}
