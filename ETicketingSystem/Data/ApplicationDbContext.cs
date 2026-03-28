using Microsoft.EntityFrameworkCore;
using ETicketingSystem.Accounting.Entities;
using ETicketingSystem.Payment.Entities;
using ETicketingSystem.Ticket.Entities;
using ETicketingSystem.Users.Entities;

namespace ETicketingSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // DbSets
    public DbSet<User> Users { get; set; }
    public DbSet<ChartOfAccount> ChartOfAccounts { get; set; }
    public DbSet<Account> Accounts { get; set; }
    public DbSet<JournalEntry> JournalEntries { get; set; }
    public DbSet<Ticket.Entities.Ticket> Tickets { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
    public DbSet<Payment.Entities.Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ChartOfAccount Configuration
        modelBuilder.Entity<ChartOfAccount>(entity =>
        {
            entity.ToTable("ChartOfAccounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Category).IsRequired();
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Account Configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Accounts");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Balance).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.Code).IsUnique();
            
            entity.HasOne(e => e.ChartOfAccount)
                .WithMany(c => c.Accounts)
                .HasForeignKey(e => e.ChartOfAccountId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // JournalEntry Configuration
        modelBuilder.Entity<JournalEntry>(entity =>
        {
            entity.ToTable("JournalEntries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SourceType).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Debit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Credit).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Description).HasMaxLength(500);
            
            entity.HasOne(e => e.Account)
                .WithMany(a => a.JournalEntries)
                .HasForeignKey(e => e.AccountId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasIndex(e => new { e.SourceType, e.SourceId });
        });

        // User Configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Ticket Configuration
        modelBuilder.Entity<Ticket.Entities.Ticket>(entity =>
        {
            entity.ToTable("Tickets");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Code).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.RowVersion).IsRowVersion(); // Optimistic concurrency
            entity.HasIndex(e => e.Code).IsUnique();
        });

        // Order Configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.OrderNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalAmount).HasColumnType("decimal(18,2)");
            entity.HasIndex(e => e.OrderNumber).IsUnique();
        });

        // OrderDetail Configuration
        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.ToTable("OrderDetails");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(e => e.TotalPrice).HasColumnType("decimal(18,2)");
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderDetails)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Ticket)
                .WithMany()
                .HasForeignKey(e => e.TicketId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment Configuration
        modelBuilder.Entity<Payment.Entities.Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.TransactionId).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Amount).HasColumnType("decimal(18,2)");
            entity.Property(e => e.ErrorMessage).HasMaxLength(500);
            entity.HasIndex(e => e.TransactionId).IsUnique();
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<Common.BaseEntity>();
        var currentUserId = 1; // TODO: Get from authenticated user context

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAt = DateTime.UtcNow;
                entry.Entity.CreatedBy = currentUserId;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAt = DateTime.UtcNow;
                entry.Entity.UpdatedBy = currentUserId;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
