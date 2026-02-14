namespace Backend.Domain;

public static class BusinessErrorCodes
{
    public const string InvalidStatusTransition = "INVALID_STATUS_TRANSITION";
    public const string PreviousOperationNotDone = "PREVIOUS_OPERATION_NOT_DONE";
    public const string LastOperationNotDone = "LAST_OPERATION_NOT_DONE";
}
