using System.Collections.ObjectModel;

namespace Backend.Domain;

public static class StatusTransitionRules
{
    public static readonly IReadOnlyDictionary<JobOrderStatus, IReadOnlySet<JobOrderStatus>> JobOrderTransitions =
        new ReadOnlyDictionary<JobOrderStatus, IReadOnlySet<JobOrderStatus>>(
            new Dictionary<JobOrderStatus, IReadOnlySet<JobOrderStatus>>
            {
                [JobOrderStatus.Draft] = new HashSet<JobOrderStatus> { JobOrderStatus.Ready, JobOrderStatus.Cancelled },
                [JobOrderStatus.Ready] = new HashSet<JobOrderStatus> { JobOrderStatus.InProgress, JobOrderStatus.Cancelled },
                [JobOrderStatus.InProgress] = new HashSet<JobOrderStatus> { JobOrderStatus.Completed, JobOrderStatus.Cancelled },
                [JobOrderStatus.Completed] = new HashSet<JobOrderStatus>(),
                [JobOrderStatus.Cancelled] = new HashSet<JobOrderStatus>()
            });

    public static readonly IReadOnlyDictionary<JobOperationStatus, IReadOnlySet<JobOperationStatus>> JobOperationTransitions =
        new ReadOnlyDictionary<JobOperationStatus, IReadOnlySet<JobOperationStatus>>(
            new Dictionary<JobOperationStatus, IReadOnlySet<JobOperationStatus>>
            {
                [JobOperationStatus.Pending] = new HashSet<JobOperationStatus> { JobOperationStatus.Ready, JobOperationStatus.Cancelled },
                [JobOperationStatus.Ready] = new HashSet<JobOperationStatus> { JobOperationStatus.InProgress, JobOperationStatus.Cancelled },
                [JobOperationStatus.InProgress] = new HashSet<JobOperationStatus> { JobOperationStatus.Done, JobOperationStatus.Cancelled },
                [JobOperationStatus.Done] = new HashSet<JobOperationStatus>(),
                [JobOperationStatus.Cancelled] = new HashSet<JobOperationStatus>()
            });

    public static bool CanTransition(JobOrderStatus from, JobOrderStatus to) =>
        JobOrderTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);

    public static bool CanTransition(JobOperationStatus from, JobOperationStatus to) =>
        JobOperationTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
