using ETicketingSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ETicketingSystem.Ticket.Services;

public class TicketService
{
    private readonly ApplicationDbContext _context;

    public TicketService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Entities.Ticket>> GetAvailableTicketsAsync()
    {
        return await _context.Tickets
            .Where(t => t.AvailableQuantity > 0)
            .ToListAsync();
    }

    public async Task<Entities.Ticket?> GetTicketByIdAsync(int ticketId)
    {
        return await _context.Tickets.FindAsync(ticketId);
    }

    /// <summary>
    /// Reserve tickets with optimistic concurrency control to prevent race conditions.
    /// </summary>
    public async Task<bool> ReserveTicketsAsync(int ticketId, int quantity)
    {
        var maxRetries = 3;
        var retryCount = 0;

        while (retryCount < maxRetries)
        {
            try
            {
                var ticket = await _context.Tickets.FindAsync(ticketId);
                
                if (ticket == null)
                {
                    throw new InvalidOperationException("Ticket not found.");
                }

                if (ticket.AvailableQuantity < quantity)
                {
                    return false; // Not enough tickets available
                }

                // Reserve the tickets
                ticket.AvailableQuantity -= quantity;
                ticket.ReservedQuantity += quantity;

                await _context.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                retryCount++;
                if (retryCount >= maxRetries)
                {
                    throw new InvalidOperationException(
                        "Unable to reserve tickets due to high concurrency. Please try again.");
                }
                
                // Reload the entity and retry
                await Task.Delay(100 * retryCount); // Exponential backoff
            }
        }

        return false;
    }

    /// <summary>
    /// Confirm ticket purchase (move from reserved to sold).
    /// </summary>
    public async Task ConfirmTicketPurchaseAsync(int ticketId, int quantity)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        
        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket not found.");
        }

        ticket.ReservedQuantity -= quantity;
        await _context.SaveChangesAsync();
    }

    /// <summary>
    /// Release reserved tickets if payment fails.
    /// </summary>
    public async Task ReleaseReservedTicketsAsync(int ticketId, int quantity)
    {
        var ticket = await _context.Tickets.FindAsync(ticketId);
        
        if (ticket == null)
        {
            throw new InvalidOperationException("Ticket not found.");
        }

        ticket.AvailableQuantity += quantity;
        ticket.ReservedQuantity -= quantity;
        await _context.SaveChangesAsync();
    }
}
