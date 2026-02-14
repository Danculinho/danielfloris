namespace OperationsService.Domain;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredUtc { get; }
    string EventType { get; }
}

public sealed record JobStatusChanged(
    Guid JobId,
    string PreviousStatus,
    string CurrentStatus,
    string? Reason = null) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
    public string EventType => nameof(JobStatusChanged);
}

public sealed record OperationStarted(
    Guid OperationId,
    Guid JobId,
    string StartedBy) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
    public string EventType => nameof(OperationStarted);
}

public sealed record OperationFinished(
    Guid OperationId,
    Guid JobId,
    bool Successful,
    string? Summary = null) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
    public string EventType => nameof(OperationFinished);
}

public sealed record InventoryDeducted(
    Guid ProductId,
    int Quantity,
    Guid OperationId) : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredUtc { get; init; } = DateTime.UtcNow;
    public string EventType => nameof(InventoryDeducted);
}
