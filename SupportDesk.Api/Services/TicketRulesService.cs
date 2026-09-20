using SupportDesk.Api.Models;

namespace SupportDesk.Api.Services;

public static class TicketRulesService
{
    public static DateTime CalculateDueDate(TicketPriority priority, DateTime createdAt)
    {
        switch (priority)
        {
            case TicketPriority.Critical:
                return createdAt.AddHours(4);
            case TicketPriority.High:
                return createdAt.AddHours(24);
            case TicketPriority.Normal:
                return createdAt.AddDays(3);
            case TicketPriority.Low:
                return createdAt.AddDays(7);
            default:
                throw new ArgumentOutOfRangeException(nameof(priority), priority, null);
        }
    }

    public static bool IsValidTransition(TicketStatus from, TicketStatus to, out string? reason)
    {
        reason = from switch
        {
            TicketStatus.New when to != TicketStatus.InProgress => "New tickets can only move to In Progress.",
            TicketStatus.InProgress when to != TicketStatus.Resolved => "In Progress tickets can only move to Resolved.",
            TicketStatus.Resolved when to != TicketStatus.Closed && to != TicketStatus.InProgress => "Resolved tickets can only move to Closed or back to In Progress.",
            TicketStatus.Closed => "Closed tickets cannot be reopened or modified.",
            TicketStatus.New or TicketStatus.InProgress or TicketStatus.Resolved => null,
            _ => throw new ArgumentOutOfRangeException(nameof(from), from, null)
        };
        return reason is null;
    }
}