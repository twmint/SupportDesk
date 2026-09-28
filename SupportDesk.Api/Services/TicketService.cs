using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.DTOs;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Services;

public enum UpdateTicketResult
{
    OK,
    NotFound,
    TicketClosed
}

public enum ChangeTicketStatusResult
{
    OK,
    NotFound,
    InvalidTransition,
    AgentRequired
}

public enum AssignAgentToTicketResult
{
    OK,
    TicketNotFound,
    AgentNotFound,
    AgentInactive,
    TicketClosed
}

public enum AddCommentResult
{
    OK,
    TicketNotFound,
    TicketClosed
}

public interface ITicketService
{
    Task<(List<Ticket> Items, int TotalCount)> GetTicketsAsync(
        string? search,
        TicketStatus? status,
        TicketPriority? priority,
        int? agentId,
        bool? overdueOnly,
        int page,
        int pageSize);

    Task<Ticket?> GetTicketByIdAsync(int id);
    Task<Ticket> CreateTicketAsync(TicketCreateDto dto);
    Task<UpdateTicketResult> UpdateTicketAsync(int id, TicketUpdateDto dto);
    Task<bool> DeleteTicketAsync(int id);
    Task<(ChangeTicketStatusResult Result, string? Reason)> ChangeStatusAsync(int id, TicketStatus newStatus);
    Task<(AssignAgentToTicketResult Result, string? Reason)> AssignAgentAsync(int id, int? agentId);
    Task<(AddCommentResult Result, Comment? Comment)> AddCommentAsync(int ticketId, CommentCreateDto dto);
}

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;

    public TicketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(List<Ticket> Items, int TotalCount)> GetTicketsAsync(
        string? search,
        TicketStatus? status,
        TicketPriority? priority,
        int? agentId,
        bool? overdueOnly,
        int page,
        int pageSize)
    {
        var query = _context.Tickets.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t =>
                t.Reference.Contains(search) ||
                t.Title.Contains(search) ||
                t.CustomerName.Contains(search));
        }

        if (status is not null) query = query.Where(t => t.Status == status);
        if (priority is not null) query = query.Where(t => t.Priority == priority);
        if (agentId is not null) query = query.Where(t => t.AgentId == agentId);

        if (overdueOnly == true)
        {
            var now = DateTime.UtcNow;
            query = query.Where(t =>
                t.Status != TicketStatus.Resolved &&
                t.Status != TicketStatus.Closed &&
                t.DueDate < now);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Ticket?> GetTicketByIdAsync(int id)
    {
        return await _context.Tickets
            .Include(t => t.Comments)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket> CreateTicketAsync(TicketCreateDto dto)
    {
        var now = DateTime.UtcNow;

        var ticket = new Ticket
        {
            Reference = await GenerateReferenceAsync(),
            Title = dto.Title,
            Description = dto.Description,
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Priority = dto.Priority,
            Status = TicketStatus.New,
            CreatedAt = now,
            DueDate = TicketRulesService.CalculateDueDate(dto.Priority, now)
        };

        _context.Tickets.Add(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<UpdateTicketResult> UpdateTicketAsync(int id, TicketUpdateDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return UpdateTicketResult.NotFound;
        if (ticket.Status == TicketStatus.Closed) return UpdateTicketResult.TicketClosed;

        if (dto.Title is not null) ticket.Title = dto.Title;
        if (dto.Description is not null) ticket.Description = dto.Description;
        if (dto.CustomerName is not null) ticket.CustomerName = dto.CustomerName;
        if (dto.CustomerEmail is not null) ticket.CustomerEmail = dto.CustomerEmail;

        if (dto.Priority is not null && dto.Priority.Value != ticket.Priority)
        {
            ticket.Priority = dto.Priority.Value;
            // Rule 1: recalculated from the ORIGINAL creation date, not now.
            ticket.DueDate = TicketRulesService.CalculateDueDate(ticket.Priority, ticket.CreatedAt);
        }

        ticket.LastModifiedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return UpdateTicketResult.OK;
    }

    public async Task<bool> DeleteTicketAsync(int id)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return false;

        _context.Tickets.Remove(ticket);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(ChangeTicketStatusResult Result, string? Reason)> ChangeStatusAsync(int id, TicketStatus newStatus)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return (ChangeTicketStatusResult.NotFound, null);

        var previousStatus = ticket.Status;

        if (!TicketRulesService.IsValidTransition(previousStatus, newStatus, out var reason))
        {
            return (ChangeTicketStatusResult.InvalidTransition, reason);
        }

        // Rule 3: cannot move to In Progress unless an active agent is assigned.
        if (newStatus == TicketStatus.InProgress)
        {
            var agent = ticket.AgentId is not null
                ? await _context.Agents.FindAsync(ticket.AgentId.Value)
                : null;

            if (agent is null || !agent.Active)
            {
                return (ChangeTicketStatusResult.AgentRequired,
                    "An active agent must be assigned before moving to In Progress.");
            }
        }

        ticket.Status = newStatus;
        ticket.LastModifiedAt = DateTime.UtcNow;

        // Rule 6: resolved/closed dates set by the system on transition.
        if (newStatus == TicketStatus.Resolved)
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }
        else if (newStatus == TicketStatus.Closed)
        {
            ticket.ClosedAt = DateTime.UtcNow;
        }
        else if (newStatus == TicketStatus.InProgress && previousStatus == TicketStatus.Resolved)
        {
            // Reopened: clear the stale resolution timestamp so a later re-resolve is accurate.
            ticket.ResolvedAt = null;
        }

        await _context.SaveChangesAsync();
        return (ChangeTicketStatusResult.OK, null);
    }

    public async Task<(AssignAgentToTicketResult Result, string? Reason)> AssignAgentAsync(int id, int? agentId)
    {
        var ticket = await _context.Tickets.FindAsync(id);
        if (ticket == null) return (AssignAgentToTicketResult.TicketNotFound, null);
        if (ticket.Status == TicketStatus.Closed)
        {
            return (AssignAgentToTicketResult.TicketClosed, "Closed tickets are read-only.");
        }

        if (agentId is not null)
        {
            var agent = await _context.Agents.FindAsync(agentId.Value);
            if (agent == null) return (AssignAgentToTicketResult.AgentNotFound, null);
            // Rule 4: an inactive agent cannot be assigned to any ticket.
            if (!agent.Active)
            {
                return (AssignAgentToTicketResult.AgentInactive, "Cannot assign an inactive agent.");
            }
        }

        ticket.AgentId = agentId;
        ticket.LastModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return (AssignAgentToTicketResult.OK, null);
    }

    public async Task<(AddCommentResult Result, Comment? Comment)> AddCommentAsync(int ticketId, CommentCreateDto dto)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null) return (AddCommentResult.TicketNotFound, null);
        if (ticket.Status == TicketStatus.Closed) return (AddCommentResult.TicketClosed, null);

        var comment = new Comment
        {
            TicketId = ticketId,
            AuthorName = dto.AuthorName,
            Body = dto.Body,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return (AddCommentResult.OK, comment);
    }

    private async Task<string> GenerateReferenceAsync()
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"REF-{year}-";

        var existingRefs = await _context.Tickets
            .Where(t => t.Reference.StartsWith(prefix))
            .Select(t => t.Reference)
            .ToListAsync();

        var maxSequence = existingRefs
            .Select(r => int.TryParse(r.AsSpan(prefix.Length), out var n) ? n : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"{prefix}{maxSequence + 1:D4}";
    }
}
