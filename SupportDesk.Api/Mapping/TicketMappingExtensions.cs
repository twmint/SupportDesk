using SupportDesk.Api.DTOs;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Mapping;

public static class TicketMappingExtensions
{
    public static TicketDto ToDto(this Ticket ticket) => new()
    {
        Id = ticket.Id,
        Reference = ticket.Reference,
        Title = ticket.Title,
        Description = ticket.Description,
        CustomerName = ticket.CustomerName,
        CustomerEmail = ticket.CustomerEmail,
        Priority = ticket.Priority,
        Status = ticket.Status,
        Comments = ticket.Comments.Select(c => c.ToDto()).ToList(),
        AgentId = ticket.AgentId,
        CreatedAt = ticket.CreatedAt,
        LastModifiedAt = ticket.LastModifiedAt,
        ResolvedAt = ticket.ResolvedAt,
        ClosedAt = ticket.ClosedAt,
        DueDate = ticket.DueDate,
        IsOverdue = ticket.IsOverdue
    };
}
