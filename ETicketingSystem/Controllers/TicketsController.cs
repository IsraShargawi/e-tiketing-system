using ETicketingSystem.Ticket.Services;
using Microsoft.AspNetCore.Mvc;

namespace ETicketingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly TicketService _ticketService;

    public TicketsController(TicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAvailableTickets()
    {
        var tickets = await _ticketService.GetAvailableTicketsAsync();
        
        var response = tickets.Select(t => new
        {
            t.Id,
            t.Code,
            t.Name,
            t.Type,
            Price = t.Price.ToString("F2"),
            t.AvailableQuantity,
            t.InitialQuantity
        });

        return Ok(response);
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _ticketService.GetTicketByIdAsync(id);
        
        if (ticket == null)
        {
            return NotFound(new { message = "Ticket not found" });
        }

        var response = new
        {
            ticket.Id,
            ticket.Code,
            ticket.Name,
            ticket.Type,
            Price = ticket.Price.ToString("F2"),
            ticket.AvailableQuantity,
            ticket.InitialQuantity
        };

        return Ok(response);
    }
}
