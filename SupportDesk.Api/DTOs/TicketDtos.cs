using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.DTOs;

public class TicketDto
{
    public int Id { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public TicketPriority Priority { get; set; }

    public TicketStatus Status { get; set; }

    public List<CommentDto> Comments { get; set; } = new();

    public int? AgentId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? LastModifiedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public DateTime? ClosedAt { get; set; }

    public DateTime DueDate { get; set; }
}

public class TicketCreateDto
{
    [Required, MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required, MaxLength(200)]
    public string CustomerName { get; set; } = string.Empty;

    [Required, MaxLength(100), EmailAddress]
    public string CustomerEmail { get; set; } = string.Empty;

    [Required]
    public TicketPriority Priority { get; set; }
}

public class TicketUpdateDto
{
    [MaxLength(200)]
    public string? Title { get; set; }

    [MaxLength(1000)]
    public string? Description { get; set; }

    [MaxLength(200)]
    public string? CustomerName { get; set; }

    [MaxLength(100), EmailAddress]
    public string? CustomerEmail { get; set; }

    public TicketPriority? Priority { get; set; }
}

public class ChangeStatusDto
{
    public TicketStatus NewStatus { get; set; }
}

public class AssignAgentDto
{
    public int? AgentId { get; set; }
}