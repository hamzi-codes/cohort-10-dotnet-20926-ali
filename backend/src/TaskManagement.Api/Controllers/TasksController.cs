using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TaskManagement.Api.Auth;
using TaskManagement.Api.Data;
using TaskManagement.Api.Dtos;
using TaskManagement.Api.Entities;

namespace TaskManagement.Api.Controllers;

/// <summary>
/// Read-only task endpoints for the initial scaffold. Create/update/delete and
/// assignment come in a follow-up feature branch.
/// Admins see every task; regular users see tasks assigned to or created by them.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TaskSummaryDto>>> GetAll(CancellationToken cancellationToken)
    {
        var tasks = await VisibleTasks()
            .OrderBy(t => t.DueDate)
            .Select(t => new TaskSummaryDto(
                t.Id, t.Title, t.Priority, t.Status, t.DueDate,
                t.Category != null ? t.Category.Name : null,
                t.AssigneeId,
                t.Assignee != null ? t.Assignee.FullName : null))
            .ToListAsync(cancellationToken);

        return Ok(tasks);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<DashboardSummaryDto>> GetSummary(CancellationToken cancellationToken)
    {
        var counts = await VisibleTasks()
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken);

        int CountOf(TaskItemStatus status) => counts.FirstOrDefault(c => c.Status == status)?.Count ?? 0;

        var pending = CountOf(TaskItemStatus.Pending);
        var inProgress = CountOf(TaskItemStatus.InProgress);
        var completed = CountOf(TaskItemStatus.Completed);
        return Ok(new DashboardSummaryDto(pending + inProgress + completed, pending, inProgress, completed));
    }

    private IQueryable<TaskItem> VisibleTasks()
    {
        var query = db.Tasks.AsNoTracking();
        if (User.IsInRole(Roles.Admin))
        {
            return query;
        }

        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("Authenticated user has no id claim."));
        return query.Where(t => t.AssigneeId == userId || t.CreatedById == userId);
    }
}
