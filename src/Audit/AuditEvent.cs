using System.Text.Json;

namespace Danielfloris.Audit;

public sealed class AuditEvent
{
    public long Id { get; set; }
    public DateTimeOffset TimestampUtc { get; set; } = DateTimeOffset.UtcNow;
    public string? UserId { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public AuditActionType ActionType { get; set; }
    public JsonDocument? OldValues { get; set; }
    public JsonDocument? NewValues { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public string? OperationName { get; set; }
}

public enum AuditActionType
{
    JobOrderStatusChanged = 1,
    OperationStarted = 2,
    OperationCompleted = 3,
    ReservationCreated = 4,
    ReservationUpdated = 5,
    InventoryMovementCreated = 6
}
