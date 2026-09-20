using Microsoft.EntityFrameworkCore;
using SupportDesk.Api.Data;
using SupportDesk.Api.DTOs;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Services;

public enum DeactivationStatus
{
    OK,
    NotFound,
    HasActiveTickets
}

public interface IAgentService
{
    Task<List<Agent>> GetAllAgentsAsync(string? search);
    Task<Agent?> GetAgentByIdAsync(int id);
    Task<Agent> CreateAgentAsync(Agent agent);
    Task<bool> UpdateAgentAsync(int id, AgentUpdateDto dto);
    Task<(DeactivationStatus Status, string? Reason)> DeactivateAgentAsync(int id);
    Task<bool> DeleteAgentAsync(int id);
}

public class AgentService : IAgentService
{
    private readonly ApplicationDbContext _context;

    public AgentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Agent>> GetAllAgentsAsync(string? search)
    {
        var query = _context.Agents.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(a => a.FullName.Contains(search) || a.Email.Contains(search));
        }

        return await query.ToListAsync();
    }

    public async Task<Agent?> GetAgentByIdAsync(int id)
    {
        return await _context.Agents.FindAsync(id);
    }

    public async Task<Agent> CreateAgentAsync(Agent agent)
    {
        _context.Agents.Add(agent);
        await _context.SaveChangesAsync();
        return agent;
    }

    public async Task<bool> UpdateAgentAsync(int id, AgentUpdateDto dto)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return false;

        if (dto.FullName is not null) agent.FullName = dto.FullName;
        if (dto.Email is not null) agent.Email = dto.Email;
        if (dto.Department is not null) agent.Department = dto.Department.Value;
        if (dto.Active is not null && dto.Active.Value == true) agent.Active = dto.Active.Value;

        await _context.SaveChangesAsync();
        return true;
    }


    public async Task<(DeactivationStatus Status, string? Reason)> DeactivateAgentAsync(int id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return (DeactivationStatus.NotFound, null);

        var assignedTickets = await _context.Tickets.Where(t => t.AgentId == id).ToListAsync();

        var inProgressCount = assignedTickets.Count(t => t.Status == TicketStatus.InProgress);
        if (inProgressCount > 0)
        {
            return (DeactivationStatus.HasActiveTickets,
                $"Cannot deactivate: {inProgressCount} ticket(s) still In Progress. Reassign them to another agent first.");
        }

        foreach (var ticket in assignedTickets)
        {
            if (ticket.Status == TicketStatus.New)
            {
                ticket.AgentId = null;
            }
            else if (ticket.Status == TicketStatus.Resolved)
            {
                ticket.Status = TicketStatus.Closed;
                ticket.ClosedAt = DateTime.UtcNow;
            }
            // Closed: already terminal, no action needed.
        }

        agent.Active = false;
        await _context.SaveChangesAsync();
        return (DeactivationStatus.OK, null);
    }

    public async Task<bool> DeleteAgentAsync(int id)
    {
        var agent = await _context.Agents.FindAsync(id);
        if (agent == null) return false;

        var assignedTickets = await _context.Tickets.Where(t => t.AgentId == id).ToListAsync();
        foreach (var ticket in assignedTickets)
        {
            ticket.AgentId = null;
        }

        _context.Agents.Remove(agent);
        await _context.SaveChangesAsync();
        return true;
    }
}