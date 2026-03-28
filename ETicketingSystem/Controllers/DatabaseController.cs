using ETicketingSystem.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ETicketingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<DatabaseController> _logger;

    public DatabaseController(ApplicationDbContext context, ILogger<DatabaseController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("migrate-and-seed")]
    public async Task<IActionResult> MigrateDatabase()
    {
        try
        {
            _logger.LogInformation("Starting fresh database migration...");

            var canConnect = await _context.Database.CanConnectAsync();
            if (!canConnect)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Cannot connect to database server",
                    error = "Please ensure SQL Server is running"
                });
            }

            // Step 1: Drop the database if it exists (clean slate)
            _logger.LogInformation("Dropping existing database if it exists...");
            await _context.Database.EnsureDeletedAsync();
            _logger.LogInformation("Database dropped successfully");

            // Step 2: Apply all migrations from scratch
            _logger.LogInformation("Applying all migrations...");
            await _context.Database.MigrateAsync();
            _logger.LogInformation("Database migration completed successfully");

            // Step 3: Seed the database with initial data
            _logger.LogInformation("Running database seeding...");
            await DbInitializer.SeedAsync(_context);
            _logger.LogInformation("Database seeding completed");

            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            var appliedList = appliedMigrations.ToList();

            return Ok(new
            {
                success = true,
                message = "Database dropped, migrated, and seeded successfully",
                appliedMigrations = appliedList,
                totalAppliedMigrations = appliedList.Count,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during database migration");
            
            var errorDetails = new
            {
                success = false,
                message = "Database migration failed",
                error = ex.Message,
                innerError = ex.InnerException?.Message,
                stackTrace = ex.StackTrace?.Split('\n').Take(5).ToArray() 
            };
            
            return StatusCode(500, errorDetails);
        }
    }
          
 
    [HttpGet("status")]
    public async Task<IActionResult> GetMigrationStatus()
    {
        try
        {
            var pendingMigrations = await _context.Database.GetPendingMigrationsAsync();
            var appliedMigrations = await _context.Database.GetAppliedMigrationsAsync();
            var canConnect = await _context.Database.CanConnectAsync();

            return Ok(new
            {
                canConnect = canConnect,
                databaseExists = canConnect,
                appliedMigrations = appliedMigrations.ToList(),
                appliedCount = appliedMigrations.Count(),
                pendingMigrations = pendingMigrations.ToList(),
                pendingCount = pendingMigrations.Count(),
                needsMigration = pendingMigrations.Any(),
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting migration status");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
    }

 
    [HttpGet("seed-status")]
    public async Task<IActionResult> GetSeedStatus()
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync();
            
            if (!canConnect)
            {
                return BadRequest(new { message = "Cannot connect to database" });
            }

            var usersCount = await _context.Users.CountAsync();
            var ticketsCount = await _context.Tickets.CountAsync();
            var coaCount = await _context.ChartOfAccounts.CountAsync();
            var accountsCount = await _context.Accounts.CountAsync();
            var ordersCount = await _context.Orders.CountAsync();
            var paymentsCount = await _context.Payments.CountAsync();

            return Ok(new
            {
                database = new
                {
                    connected = true,
                    name = _context.Database.GetDbConnection().Database
                },
                seedStatus = new
                {
                    users = usersCount,
                    tickets = ticketsCount,
                    chartOfAccounts = coaCount,
                    accounts = accountsCount,
                    orders = ordersCount,
                    payments = paymentsCount
                },
                isSeeded = usersCount > 0 && ticketsCount > 0 && accountsCount > 0,
                timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting seed status");
            return StatusCode(500, new
            {
                success = false,
                error = ex.Message
            });
        }
    }
}
