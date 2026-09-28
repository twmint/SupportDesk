using Microsoft.AspNetCore.Mvc;
using SupportDesk.Api.DTOs;
using SupportDesk.Api.Mapping;
using SupportDesk.Api.Models;
using SupportDesk.Api.Services;

namespace SupportDesk.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketsController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<TicketDto>>> GetTickets(
            [FromQuery] string? search,
            [FromQuery] TicketStatus? status,
            [FromQuery] TicketPriority? priority,
            [FromQuery] int? agentId,
            [FromQuery] bool? overdueOnly,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var (items, totalCount) = await _ticketService.GetTicketsAsync(
                search, status, priority, agentId, overdueOnly, page, pageSize);

            return Ok(new PagedResult<TicketDto>
            {
                Items = items.Select(t => t.ToDto()).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<TicketDto>> GetTicket(int id)
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            return Ok(ticket.ToDto());
        }

        [HttpPost]
        public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] TicketCreateDto dto)
        {
            var ticket = await _ticketService.CreateTicketAsync(dto);
            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket.ToDto());
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTicket(int id, [FromBody] TicketUpdateDto dto)
        {
            var result = await _ticketService.UpdateTicketAsync(id, dto);
            return result switch
            {
                UpdateTicketResult.NotFound => NotFound(),
                UpdateTicketResult.TicketClosed => BadRequest(new { error = "Closed tickets are read-only." }),
                _ => NoContent()
            };
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTicket(int id)
        {
            var deleted = await _ticketService.DeleteTicketAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> ChangeStatus(int id, [FromBody] ChangeStatusDto dto)
        {
            var (result, reason) = await _ticketService.ChangeStatusAsync(id, dto.NewStatus);
            return result switch
            {
                ChangeTicketStatusResult.NotFound => NotFound(),
                ChangeTicketStatusResult.InvalidTransition => BadRequest(new { error = reason }),
                ChangeTicketStatusResult.AgentRequired => BadRequest(new { error = reason }),
                _ => NoContent()
            };
        }

        [HttpPatch("{id}/assign")]
        public async Task<IActionResult> AssignAgent(int id, [FromBody] AssignAgentDto dto)
        {
            var (result, reason) = await _ticketService.AssignAgentAsync(id, dto.AgentId);
            return result switch
            {
                AssignAgentToTicketResult.TicketNotFound => NotFound(),
                AssignAgentToTicketResult.AgentNotFound => BadRequest(new { error = "Agent not found." }),
                AssignAgentToTicketResult.AgentInactive => BadRequest(new { error = reason }),
                AssignAgentToTicketResult.TicketClosed => BadRequest(new { error = reason }),
                _ => NoContent()
            };
        }

        [HttpPost("{id}/comments")]
        public async Task<ActionResult<CommentDto>> AddComment(int id, [FromBody] CommentCreateDto dto)
        {
            var (result, comment) = await _ticketService.AddCommentAsync(id, dto);
            return result switch
            {
                AddCommentResult.TicketNotFound => NotFound(),
                AddCommentResult.TicketClosed => BadRequest(new { error = "Closed tickets are read-only, cannot add comments." }),
                _ => CreatedAtAction(nameof(GetTicket), new { id }, comment!.ToDto())
            };
        }
    }
}
