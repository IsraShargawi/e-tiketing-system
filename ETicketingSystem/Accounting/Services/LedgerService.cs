using ETicketingSystem.Accounting.Entities;
using ETicketingSystem.Common;
using ETicketingSystem.Data;
using Microsoft.EntityFrameworkCore;

namespace ETicketingSystem.Accounting.Services;

public class LedgerService
{
    private readonly ApplicationDbContext _context;

    public LedgerService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task RecordPaymentLedgerAsync(int paymentId, decimal amount, PaymentMethod paymentMethod)
    {
        var cashAccountCode = paymentMethod == PaymentMethod.CreditCard ? "1100" : "1110";
        var revenueAccountCode = "4100";

        var cashAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Code == cashAccountCode);
            
        var revenueAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Code == revenueAccountCode);

        if (cashAccount == null || revenueAccount == null)
        {
            throw new InvalidOperationException("Required accounts not found in the system.");
        }

        var journalEntries = new List<JournalEntry>
        {
            new()
            {
                SourceId = paymentId,
                SourceType = "Payment",
                AccountId = cashAccount.Id,
                Debit = amount,
                Credit = 0,
                Description = $"Payment received via {paymentMethod}",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            },
            new()
            {
                SourceId = paymentId,
                SourceType = "Payment",
                AccountId = revenueAccount.Id,
                Debit = 0,
                Credit = amount,
                Description = "Ticket sales revenue",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            }
        };

        _context.JournalEntries.AddRange(journalEntries);

        // Update account balances
        cashAccount.Balance += amount;
        revenueAccount.Balance += amount;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> ValidateLedgerBalanceAsync()
    {
        var totalDebits = await _context.JournalEntries.SumAsync(j => j.Debit);
        var totalCredits = await _context.JournalEntries.SumAsync(j => j.Credit);

        return totalDebits == totalCredits;
    }

    public async Task<object> GetLedgerSummaryAsync()
    {
        var accounts = await _context.Accounts
            .Include(a => a.ChartOfAccount)
            .Select(a => new 
            {
                a.Code,
                a.Name,
                Category = a.ChartOfAccount.Category,
                a.Balance
            })
            .ToListAsync();

        var totalDebits = await _context.JournalEntries.SumAsync(j => j.Debit);
        var totalCredits = await _context.JournalEntries.SumAsync(j => j.Credit);

        return new
        {
            Accounts = accounts,
            TotalDebits = totalDebits,
            TotalCredits = totalCredits,
            IsBalanced = totalDebits == totalCredits
        };
    }
}
