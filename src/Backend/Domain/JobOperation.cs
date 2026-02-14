namespace Backend.Domain;

public class JobOperation
{
    public JobOperation(Guid id, int sequence)
    {
        Id = id;
        Sequence = sequence;
        Status = JobOperationStatus.Pending;
    }

    public Guid Id { get; }
    public int Sequence { get; }
    public JobOperationStatus Status { get; private set; }

    internal OperationResult ChangeStatus(JobOperationStatus targetStatus, JobOperation? previousOperation)
    {
        if (!StatusTransitionRules.CanTransition(Status, targetStatus))
        {
            return OperationResult.Fail(
                BusinessErrorCodes.InvalidStatusTransition,
                $"Transition from {Status} to {targetStatus} is not allowed.");
        }

        if (targetStatus == JobOperationStatus.InProgress &&
            previousOperation is not null &&
            previousOperation.Status != JobOperationStatus.Done)
        {
            return OperationResult.Fail(
                BusinessErrorCodes.PreviousOperationNotDone,
                "Previous operation must be Done before starting this operation.");
        }

        Status = targetStatus;
        return OperationResult.Success();
    }
}
