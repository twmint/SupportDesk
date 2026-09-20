using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.Models;

public enum TicketPriority
{
    Critical,
    High,
    Normal,
    Low
}

public enum TicketStatus
{
    New,
    InProgress,
    Resolved,
    Closed
}

public class Ticket
{
    public int Id { get; set; }
    
    [Required, MaxLength(50)]
    public string Reference { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(100), EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; } = TicketStatus.New;

    public List<Comment> Comments { get; set; } = new();

    public int? AgentId { get; set; }

    public Agent? Agent { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? LastModifiedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime DueDate { get; set; }

    public bool IsOverdue => Status != TicketStatus.Resolved && Status != TicketStatus.Closed && DueDate < DateTime.UtcNow;
}