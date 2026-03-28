using ETicketingSystem.Accounting.Entities;
using ETicketingSystem.Common;
using ETicketingSystem.Users.Entities;
using Microsoft.EntityFrameworkCore;

namespace ETicketingSystem.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        // Seed User
        if (!await context.Users.AnyAsync())
        {
            var user = new User
            {
                Username = "testuser",
                Email = "test@example.com",
                FullName = "Test User",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };
            context.Users.Add(user);
            await context.SaveChangesAsync();
        }

        // Seed Chart of Accounts
        if (!await context.ChartOfAccounts.AnyAsync())
        {
            var coas = new List<ChartOfAccount>
            {
                new() { Code = "1000", Name = "Assets", Category = AccountCategory.Asset, CreatedAt = DateTime.UtcNow, CreatedBy = 1 },
                new() { Code = "4000", Name = "Revenue", Category = AccountCategory.Revenue, CreatedAt = DateTime.UtcNow, CreatedBy = 1 }
            };
            context.ChartOfAccounts.AddRange(coas);
            await context.SaveChangesAsync();
        }

        // Seed Accounts
        if (!await context.Accounts.AnyAsync())
        {
            var assetCoa = await context.ChartOfAccounts.FirstAsync(c => c.Code == "1000");
            var revenueCoa = await context.ChartOfAccounts.FirstAsync(c => c.Code == "4000");

            var accounts = new List<Account>
            {
                new() 
                { 
                    ChartOfAccountId = assetCoa.Id,
                    Code = "1100", 
                    Name = "Cash - Credit Card", 
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow, 
                    CreatedBy = 1 
                },
                new() 
                { 
                    ChartOfAccountId = assetCoa.Id,
                    Code = "1110", 
                    Name = "Cash - QR Payment Gateway", 
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow, 
                    CreatedBy = 1 
                },
                new() 
                { 
                    ChartOfAccountId = revenueCoa.Id,
                    Code = "4100", 
                    Name = "Ticket Sales Revenue", 
                    Balance = 0,
                    CreatedAt = DateTime.UtcNow, 
                    CreatedBy = 1 
                }
            };
            context.Accounts.AddRange(accounts);
            await context.SaveChangesAsync();
        }

        // Seed Tickets
        if (!await context.Tickets.AnyAsync())
        {
            var tickets = new List<Ticket.Entities.Ticket>
            {
                new() 
                { 
                    Code = "GOLD", 
                    Type = TicketType.Gold,
                    Name = "Gold Ticket", 
                    Price = 100m, 
                    InitialQuantity = 100,
                    AvailableQuantity = 100,
                    ReservedQuantity = 0,
                    CreatedAt = DateTime.UtcNow, 
                    CreatedBy = 1 
                },
                new() 
                { 
                    Code = "PREMIUM", 
                    Type = TicketType.Premium,
                    Name = "Premium Ticket", 
                    Price = 200m, 
                    InitialQuantity = 50,
                    AvailableQuantity = 50,
                    ReservedQuantity = 0,
                    CreatedAt = DateTime.UtcNow, 
                    CreatedBy = 1 
                },
                new() 
                { 
                    Code = "VIP", 
                    Type = TicketType.VIP,
                    Name = "VIP Ticket", 
                    Price = 500m, 
                    InitialQuantity = 20,
                    AvailableQuantity = 20,
                    ReservedQuantity = 0,
                    CreatedAt = DateTime.UtcNow, 
                    CreatedBy = 1 
                }
            };
            context.Tickets.AddRange(tickets);
            await context.SaveChangesAsync();
        }
    }
}
