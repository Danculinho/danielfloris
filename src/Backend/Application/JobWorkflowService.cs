using Backend.Domain;

namespace Backend.Application;

public class JobWorkflowService
{
    public OperationResult UpdateJobOperationStatus(JobOrder jobOrder, Guid operationId, JobOperationStatus targetStatus)
    {
        var orderedOperations = jobOrder.Operations.OrderBy(x => x.Sequence).ToList();
        var operationIndex = orderedOperations.FindIndex(o => o.Id == operationId);

        if (operationIndex < 0)
        {
            return OperationResult.Fail(
                BusinessErrorCodes.InvalidStatusTransition,
                $"Operation {operationId} was not found in job order {jobOrder.Id}.");
        }

        var previousOperation = operationIndex > 0 ? orderedOperations[operationIndex - 1] : null;
        var operation = orderedOperations[operationIndex];

        return operation.ChangeStatus(targetStatus, previousOperation);
    }

    public OperationResult UpdateJobOrderStatus(JobOrder jobOrder, JobOrderStatus targetStatus)
    {
        return jobOrder.ChangeStatus(targetStatus);
    }
}
