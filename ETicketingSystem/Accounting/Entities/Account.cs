using ETicketingSystem.Common;

namespace ETicketingSystem.Accounting.Entities;

public class Account : BaseEntity
{
    public int ChartOfAccountId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Balance { get; set; }
    
    // Navigation properties
    public ChartOfAccount ChartOfAccount { get; set; } = null!;
    public ICollection<JournalEntry> JournalEntries { get; set; } = new List<JournalEntry>();
}
