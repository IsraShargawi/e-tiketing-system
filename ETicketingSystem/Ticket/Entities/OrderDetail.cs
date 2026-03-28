using ETicketingSystem.Common;

namespace ETicketingSystem.Ticket.Entities;

public class OrderDetail : BaseEntity
{
    public int OrderId { get; set; }
    public int TicketId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    
    // Navigation properties
    public Order Order { get; set; } = null!;
    public Ticket Ticket { get; set; } = null!;
}
