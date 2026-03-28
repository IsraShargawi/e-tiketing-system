using ETicketingSystem.Common;

namespace ETicketingSystem.Ticket.Entities;

public class Order : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public int UserId { get; set; }
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; }
    
    // Navigation properties
    public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
