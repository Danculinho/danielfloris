using System.Text.Json;

namespace Danielfloris.Audit;

public interface IAuditLogger
{
    Task LogJobOrderStatusChangeAsync(string jobOrderId, string? userId, string oldStatus, string newStatus, CancellationToken cancellationToken = default);
    Task LogOperationAsync(string operationName, bool started, string? userId, object? payload = null, CancellationToken cancellationToken = default);
    Task LogReservationCreatedAsync(string reservationId, string? userId, object newValues, CancellationToken cancellationToken = default);
    Task LogReservationUpdatedAsync(string reservationId, string? userId, object oldValues, object newValues, CancellationToken cancellationToken = default);
    Task LogInventoryMovementAsync(string movementId, string? userId, object movement, CancellationToken cancellationToken = default);
}

public sealed class AuditLogger : IAuditLogger
{
    private readonly IAuditSink _auditSink;
    private readonly ICorrelationContext _correlationContext;

    public AuditLogger(IAuditSink auditSink, ICorrelationContext correlationContext)
    {
        _auditSink = auditSink;
        _correlationContext = correlationContext;
    }

    public Task LogJobOrderStatusChangeAsync(string jobOrderId, string? userId, string oldStatus, string newStatus, CancellationToken cancellationToken = default)
        => WriteAsync(
            entityName: "JobOrder",
            entityId: jobOrderId,
            actionType: AuditActionType.JobOrderStatusChanged,
            userId: userId,
            oldValues: new { Status = oldStatus },
            newValues: new { Status = newStatus },
            cancellationToken: cancellationToken);

    public Task LogOperationAsync(string operationName, bool started, string? userId, object? payload = null, CancellationToken cancellationToken = default)
        => WriteAsync(
            entityName: "Operation",
            entityId: operationName,
            actionType: started ? AuditActionType.OperationStarted : AuditActionType.OperationCompleted,
            userId: userId,
            oldValues: null,
            newValues: payload,
            operationName: operationName,
            cancellationToken: cancellationToken);

    public Task LogReservationCreatedAsync(string reservationId, string? userId, object newValues, CancellationToken cancellationToken = default)
        => WriteAsync(
            entityName: "Reservation",
            entityId: reservationId,
            actionType: AuditActionType.ReservationCreated,
            userId: userId,
            oldValues: null,
            newValues: newValues,
            cancellationToken: cancellationToken);

    public Task LogReservationUpdatedAsync(string reservationId, string? userId, object oldValues, object newValues, CancellationToken cancellationToken = default)
        => WriteAsync(
            entityName: "Reservation",
            entityId: reservationId,
            actionType: AuditActionType.ReservationUpdated,
            userId: userId,
            oldValues: oldValues,
            newValues: newValues,
            cancellationToken: cancellationToken);

    public Task LogInventoryMovementAsync(string movementId, string? userId, object movement, CancellationToken cancellationToken = default)
        => WriteAsync(
            entityName: "InventoryMovement",
            entityId: movementId,
            actionType: AuditActionType.InventoryMovementCreated,
            userId: userId,
            oldValues: null,
            newValues: movement,
            cancellationToken: cancellationToken);

    private Task WriteAsync(
        string entityName,
        string entityId,
        AuditActionType actionType,
        string? userId,
        object? oldValues,
        object? newValues,
        string? operationName = null,
        CancellationToken cancellationToken = default)
    {
        var auditEvent = new AuditEvent
        {
            TimestampUtc = DateTimeOffset.UtcNow,
            UserId = userId,
            EntityName = entityName,
            EntityId = entityId,
            ActionType = actionType,
            CorrelationId = _correlationContext.CorrelationId,
            OperationName = operationName,
            OldValues = oldValues is null ? null : JsonSerializer.SerializeToDocument(oldValues),
            NewValues = newValues is null ? null : JsonSerializer.SerializeToDocument(newValues)
        };

        return _auditSink.WriteAsync(auditEvent, cancellationToken);
    }
}
