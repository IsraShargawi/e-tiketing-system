using ETicketingSystem.Data;
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

    }
}