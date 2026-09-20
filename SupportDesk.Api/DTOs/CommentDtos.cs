using System.ComponentModel.DataAnnotations;

namespace SupportDesk.Api.DTOs;

public class CommentDto
{
    public int Id { get; set; }

    public string AuthorName { get; set; } = string.Empty;

    public string Body { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
}

public class CommentCreateDto
{
    [Required, MaxLength(200)]
    public string AuthorName { get; set; } = string.Empty;

    [Required, MaxLength(1000)]
    public string Body { get; set; } = string.Empty;
}
