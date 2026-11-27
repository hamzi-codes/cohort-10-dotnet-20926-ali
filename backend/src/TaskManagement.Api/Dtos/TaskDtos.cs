using TaskManagement.Api.Entities;

namespace TaskManagement.Api.Dtos;

public record TaskSummaryDto(
    int Id,
    string Title,
    TaskPriority Priority,
    TaskItemStatus Status,
    DateTime? DueDate,
    string? CategoryName,
    int? AssigneeId,
    string? AssigneeName);

public record DashboardSummaryDto(int Total, int Pending, int InProgress, int Completed);
