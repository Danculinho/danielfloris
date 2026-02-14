namespace Danielfloris.Audit;

public sealed class InMemoryAuditSink : IAuditSink
{
    private readonly List<AuditEvent> _events = [];

    public IReadOnlyList<AuditEvent> Events => _events;

    public Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        _events.Add(auditEvent);
        return Task.CompletedTask;
    }
}
