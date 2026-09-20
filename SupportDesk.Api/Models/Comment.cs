using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.Models;

public class Comment
{
    public int Id { get; set; }

    public int TicketId { get; set; }

    public Ticket Ticket { get; set; } = null!;

    [Required, MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}