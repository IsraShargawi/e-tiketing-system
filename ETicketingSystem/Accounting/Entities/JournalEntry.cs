using ETicketingSystem.Common;

namespace ETicketingSystem.Accounting.Entities;

public class JournalEntry : BaseEntity
{
    public int SourceId { get; set; }
    public string SourceType { get; set; } = string.Empty; // "Payment", "Order", etc.
    public int AccountId { get; set; }
    public decimal Debit { get; set; }
    public decimal Credit { get; set; }
    public string Description { get; set; } = string.Empty;
    
    // Navigation properties
    public Account Account { get; set; } = null!;
}
