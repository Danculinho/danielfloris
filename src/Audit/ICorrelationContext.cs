namespace Danielfloris.Audit;

public interface ICorrelationContext
{
    string CorrelationId { get; }
}
