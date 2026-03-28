using ETicketingSystem.Common;
using System.ComponentModel.DataAnnotations;

namespace ETicketingSystem.Ticket.Entities;

public class Ticket : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public TicketType Type { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int InitialQuantity { get; set; }
    public int AvailableQuantity { get; set; }
    public int ReservedQuantity { get; set; }
    
    [Timestamp]
    public byte[] RowVersion { get; set; } = null!; // For optimistic concurrency
}
