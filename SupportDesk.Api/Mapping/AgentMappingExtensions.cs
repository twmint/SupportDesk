using SupportDesk.Api.DTOs;
using SupportDesk.Api.Models;

namespace SupportDesk.Api.Mapping;

public static class AgentMappingExtensions
{
    public static AgentDto ToDto(this Agent agent) => new()
    {
        Id = agent.Id,
        FullName = agent.FullName,
        Email = agent.Email,
        Department = agent.Department,
        Active = agent.Active
    };

    public static Agent ToEntity(this AgentCreateDto dto) => new()
    {
        FullName = dto.FullName,
        Email = dto.Email,
        Department = dto.Department
    };
}
