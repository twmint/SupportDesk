using SupportDesk.Api.DTOs;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Mapping;

public static class CommentMappingExtensions
{
    public static CommentDto ToDto(this Comment comment) => new()
    {
        Id = comment.Id,
        AuthorName = comment.AuthorName,
        Body = comment.Body,
        CreatedAt = comment.CreatedAt
    };
}
