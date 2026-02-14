namespace Backend.Domain;

public class JobOrder
{
    private readonly List<JobOperation> _operations = [];

    public JobOrder(Guid id)
    {
        Id = id;
        Status = JobOrderStatus.Draft;
    }

    public Guid Id { get; }
    public JobOrderStatus Status { get; private set; }
    public IReadOnlyList<JobOperation> Operations => _operations;

    public void AddOperation(JobOperation operation) => _operations.Add(operation);

    internal OperationResult ChangeStatus(JobOrderStatus targetStatus)
    {
        if (!StatusTransitionRules.CanTransition(Status, targetStatus))
        {
            return OperationResult.Fail(
                BusinessErrorCodes.InvalidStatusTransition,
                $"Transition from {Status} to {targetStatus} is not allowed.");
        }

        if (targetStatus == JobOrderStatus.Completed)
        {
            var lastOperation = _operations.OrderBy(o => o.Sequence).LastOrDefault();
            if (lastOperation is null || lastOperation.Status != JobOperationStatus.Done)
            {
                return OperationResult.Fail(
                    BusinessErrorCodes.LastOperationNotDone,
                    "Job order can be completed only when last operation is Done.");
            }
        }

        Status = targetStatus;
        return OperationResult.Success();
    }
}
