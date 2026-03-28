using ETicketingSystem.Common;

namespace ETicketingSystem.Accounting.Entities;

public class ChartOfAccount : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public AccountCategory Category { get; set; }
    
    // Navigation properties
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
}
