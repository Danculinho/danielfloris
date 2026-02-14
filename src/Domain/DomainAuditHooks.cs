using Danielfloris.Audit;

namespace Danielfloris.Domain;

public sealed class DomainAuditHooks
{
    private readonly IAuditLogger _auditLogger;

    public DomainAuditHooks(IAuditLogger auditLogger)
    {
        _auditLogger = auditLogger;
    }

    public Task OnJobOrderStatusChangedAsync(string jobOrderId, string? userId, string oldStatus, string newStatus, CancellationToken cancellationToken = default)
        => _auditLogger.LogJobOrderStatusChangeAsync(jobOrderId, userId, oldStatus, newStatus, cancellationToken);

    public Task OnOperationStartedAsync(string operationName, string? userId, object? contextPayload = null, CancellationToken cancellationToken = default)
        => _auditLogger.LogOperationAsync(operationName, started: true, userId, contextPayload, cancellationToken);

    public Task OnOperationCompletedAsync(string operationName, string? userId, object? contextPayload = null, CancellationToken cancellationToken = default)
        => _auditLogger.LogOperationAsync(operationName, started: false, userId, contextPayload, cancellationToken);

    public Task OnReservationCreatedAsync(string reservationId, string? userId, object newReservation, CancellationToken cancellationToken = default)
        => _auditLogger.LogReservationCreatedAsync(reservationId, userId, newReservation, cancellationToken);

    public Task OnReservationUpdatedAsync(string reservationId, string? userId, object oldReservation, object newReservation, CancellationToken cancellationToken = default)
        => _auditLogger.LogReservationUpdatedAsync(reservationId, userId, oldReservation, newReservation, cancellationToken);

    public Task OnInventoryMovementAsync(string movementId, string? userId, object movementPayload, CancellationToken cancellationToken = default)
        => _auditLogger.LogInventoryMovementAsync(movementId, userId, movementPayload, cancellationToken);
}
