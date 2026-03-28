using ETicketingSystem.Accounting.Entities;
using ETicketingSystem.Accounting.Services;
using ETicketingSystem.Common;
using ETicketingSystem.Data;
using ETicketingSystem.Payment.Handlers;
using ETicketingSystem.Ticket.Services;
using Microsoft.EntityFrameworkCore;

namespace ETicketingSystem.UnitTest
{
    public class UnitTest1
    {
        private ApplicationDbContext GetDbContext()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            var context = new ApplicationDbContext(options);
            return context;
        }

        [Fact]
        public void Test1()
        {
            Assert.True(true);
        }

        //Todo
        //Test concurrecy case
        //Test double entyr bookkeeping
        [Fact]
        public async Task RecordPayment_ShouldCreateDoubleEntry()
        {
            var context = GetDbContext();
            var ledgerService = new LedgerService(context);

            // Setup chart of accounts
            var cashCOA = new ChartOfAccount
            {
                Code = "1100",
                Name = "Cash",
                Category = AccountCategory.Asset,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            var revenueCOA = new ChartOfAccount
            {
                Code = "4100",
                Name = "Revenue",
                Category = AccountCategory.Revenue,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            context.ChartOfAccounts.AddRange(cashCOA, revenueCOA);
            await context.SaveChangesAsync();

            var cashAccount = new Account
            {
                ChartOfAccountId = cashCOA.Id,
                Code = "1100",
                Name = "Cash Account",
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            var revenueAccount = new Account
            {
                ChartOfAccountId = revenueCOA.Id,
                Code = "4100",
                Name = "Sales Revenue",
                Balance = 0,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            context.Accounts.AddRange(cashAccount, revenueAccount);
            await context.SaveChangesAsync();

            await ledgerService.RecordPaymentLedgerAsync(1, 100m, PaymentMethod.CreditCard);

            var entries = await context.JournalEntries.ToListAsync();
            Assert.Equal(2, entries.Count);

            var debitEntry = entries.First(e => e.Debit > 0);
            var creditEntry = entries.First(e => e.Credit > 0);

            Assert.Equal(100m, debitEntry.Debit);
            Assert.Equal(100m, creditEntry.Credit);
        }

        //Test jounal entry balance check
        [Fact]
        public async Task ValidateLedgerBalance_ShouldBeBalanced()
        {
            var context = GetDbContext();
            var ledgerService = new LedgerService(context);

            context.JournalEntries.AddRange(
                new JournalEntry
                {
                    SourceId = 1,
                    SourceType = "Payment",
                    AccountId = 1,
                    Debit = 100,
                    Credit = 0,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                },
                new JournalEntry
                {
                    SourceId = 1,
                    SourceType = "Payment",
                    AccountId = 2,
                    Debit = 0,
                    Credit = 100,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = 1
                }
            );

            await context.SaveChangesAsync();

            var isBalanced = await ledgerService.ValidateLedgerBalanceAsync();

            Assert.True(isBalanced);
        }

        //QRCode delay test
        [Fact]
        public async Task QRScan_ShouldTakeAtLeast8Seconds()
        {
            var handler = new QRScanHandler();
            var payment = new ETicketingSystem.Payment.Entities.Payment
            {
                TransactionId = "TXN-123",
                Amount = 100,
                Method = PaymentMethod.QRScan,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = 1
            };

            var startTime = DateTime.UtcNow;
            var result = await handler.ProcessPaymentAsync(payment);
            var duration = (DateTime.UtcNow - startTime).TotalSeconds;

            Assert.True(result.Success);
            Assert.True(duration >= 8, $"Expected at least 8 seconds, but took {duration:F2} seconds");
        }

    }
}