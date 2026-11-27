namespace TaskManagement.Api.Entities;

/// <summary>
/// Workflow status of a task. Named TaskItemStatus to avoid clashing with
/// System.Threading.Tasks.TaskStatus.
/// </summary>
public enum TaskItemStatus
{
    Pending = 0,
    InProgress = 1,
    Completed = 2
}
