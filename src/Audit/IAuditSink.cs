namespace Danielfloris.Audit;

public interface IAuditSink
{
    Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);
}
