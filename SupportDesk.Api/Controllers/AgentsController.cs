using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.DTOs;
using SupportDesk.Api.Mapping;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly IAgentService _agentService;

        public AgentsController(IAgentService agentService)
        {
            _agentService = agentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AgentDto>>> GetAgents([FromQuery] string? search)
        {
            var agents = await _agentService.GetAllAgentsAsync(search);
            return Ok(agents.Select(a => a.ToDto()));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgentDto>> GetAgent(int id)
        {
            var agent = await _agentService.GetAgentByIdAsync(id);
            if (agent == null)
            {
                return NotFound();
            }
            return Ok(agent.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<AgentDto>> CreateAgent([FromBody] AgentCreateDto agentCreateDto)
        {
            var agent = agentCreateDto.ToEntity();
            var createdAgent = await _agentService.CreateAgentAsync(agent);
            return CreatedAtAction(nameof(GetAgent), new { id = createdAgent.Id }, createdAgent.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgent(int id, [FromBody] AgentUpdateDto agentUpdateDto)
        {
            var updated = await _agentService.UpdateAgentAsync(id, agentUpdateDto);
            if (!updated)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeactivateAgent(int id)
        {
            var (status, reason) = await _agentService.DeactivateAgentAsync(id);
            return status switch
            {
                DeactivationStatus.NotFound => NotFound(),
                DeactivationStatus.HasActiveTickets => BadRequest(new { error = reason }),
                _ => NoContent()
            };
        }

        [HttpDelete("{id}/hard")]
        public async Task<IActionResult> DeleteAgent(int id)
        {
            var deleted = await _agentService.DeleteAgentAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
