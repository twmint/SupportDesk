using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.Models;

public enum Department
{
    Technical,
    Billing,
    General
}

public class Agent
{
    public int Id { get; set; }

    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(320)]
    public string Email { get; set; } = string.Empty;

    public Department Department { get; set; }

    public bool Active { get; set; } = true;

    public List<Ticket> Tickets { get; set; } = new();
}
