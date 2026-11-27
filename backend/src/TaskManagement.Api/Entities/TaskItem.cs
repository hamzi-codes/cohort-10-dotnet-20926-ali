namespace TaskManagement.Api.Entities;

/// <summary>
/// A task in the system. Named TaskItem to avoid clashing with System.Threading.Tasks.Task.
/// </summary>
public class TaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Pending;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    public int? AssigneeId { get; set; }
    public User? Assignee { get; set; }

    public int CreatedById { get; set; }
    public User? CreatedBy { get; set; }
}
