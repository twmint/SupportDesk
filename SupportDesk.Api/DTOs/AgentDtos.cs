using System.ComponentModel.DataAnnotations;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.DTOs;

public class AgentDto
{
    public int Id { get; set; }

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public Department Department { get; set; }

    public bool Active { get; set; } = true;
}

public class AgentCreateDto
{
    [Required, MaxLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required, MaxLength(320), EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public Department Department { get; set; }
}

public class AgentUpdateDto
{
    [MaxLength(200)]
    public string? FullName { get; set; }

    [MaxLength(320), EmailAddress]
    public string? Email { get; set; }

    public Department? Department { get; set; }

    public bool? Active { get; set; }
}