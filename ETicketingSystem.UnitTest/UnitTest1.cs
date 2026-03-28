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
        //Test jounal entry balance check
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