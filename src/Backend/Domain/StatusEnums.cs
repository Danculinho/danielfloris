namespace Backend.Domain;

public enum JobOrderStatus
{
    Draft,
    Ready,
    InProgress,
    Completed,
    Cancelled
}

public enum JobOperationStatus
{
    Pending,
    Ready,
    InProgress,
    Done,
    Cancelled
}
