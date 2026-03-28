using ETicketingSystem.Accounting.Services;
using ETicketingSystem.Common;
using ETicketingSystem.Data;
using ETicketingSystem.Payment.Services;
using ETicketingSystem.Ticket.Entities;
using ETicketingSystem.Ticket.Services;
using Microsoft.EntityFrameworkCore;

namespace ETicketingSystem.Ticket.Services;

public class OrderService
{
    private readonly ApplicationDbContext _context;
    private readonly TicketService _ticketService;
    private readonly PaymentService _paymentService;
    private readonly LedgerService _ledgerService;

    public OrderService(
        ApplicationDbContext context,
        TicketService ticketService,
        PaymentService paymentService,
        LedgerService ledgerService)
    {
        _context = context;
        _ticketService = ticketService;
        _paymentService = paymentService;
        _ledgerService = ledgerService;
    }

    /// <summary>
    /// Creates an order and processes payment with double-entry ledger recording.
    /// This method ensures data integrity even if the system crashes mid-transaction.
    /// </summary>
    public async Task<CheckoutResult> CreateOrderAndProcessPaymentAsync(
        int userId, 
        List<OrderItemRequest> items, 
        PaymentMethod paymentMethod)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        
        return await strategy.ExecuteAsync(async () =>
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
            // Step 1: Validate and reserve tickets (with concurrency control)
            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();

            foreach (var item in items)
            {
                var ticket = await _ticketService.GetTicketByIdAsync(item.TicketId);
                
                if (ticket == null)
                {
                    return new CheckoutResult 
                    { 
                        Success = false, 
                        ErrorMessage = $"Ticket with ID {item.TicketId} not found." 
                    };
                }

                // Reserve tickets (handles race conditions with optimistic concurrency)
                var reserved = await _ticketService.ReserveTicketsAsync(item.TicketId, item.Quantity);
                
                if (!reserved)
                {
                    return new CheckoutResult 
                    { 
                        Success = false, 
                        ErrorMessage = $"Not enough {ticket.Name} tickets available. Only {ticket.AvailableQuantity} left." 
                    };
                }

                var itemTotal = ticket.Price * item.Quantity;
                totalAmount += itemTotal;

                orderDetails.Add(new OrderDetail
                {
                    TicketId = ticket.Id,
                    Quantity = item.Quantity,
                    UnitPrice = ticket.Price,
                    TotalPrice = itemTotal,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = userId
                });
            }

            // Step 2: Create Order
            var order = new Order
            {
                OrderNumber = GenerateOrderNumber(),
                UserId = userId,
                TotalAmount = totalAmount,
                Status = OrderStatus.Pending,
                OrderDetails = orderDetails,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            // Step 3: Create Payment Record
            var payment = new Payment.Entities.Payment
            {
                TransactionId = GenerateTransactionId(),
                OrderId = order.Id,
                Amount = totalAmount,
                Method = paymentMethod,
                Status = PaymentStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = userId
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Step 4: Process Payment (Credit Card instant, QR 8-second delay)
            var paymentResult = await _paymentService.ProcessPaymentAsync(payment);

            if (!paymentResult.Success)
            {
                // Payment failed - rollback and release reserved tickets
                payment.Status = PaymentStatus.Failed;
                payment.ErrorMessage = paymentResult.ErrorMessage;
                order.Status = OrderStatus.Cancelled;
                
                await _context.SaveChangesAsync();
                
                // Release reserved tickets
                foreach (var item in items)
                {
                    await _ticketService.ReleaseReservedTicketsAsync(item.TicketId, item.Quantity);
                }

                await transaction.RollbackAsync();
                
                return new CheckoutResult 
                { 
                    Success = false, 
                    ErrorMessage = paymentResult.ErrorMessage ?? "Payment processing failed." 
                };
            }

            // Step 5: Update Payment Status
            payment.Status = PaymentStatus.Completed;
            payment.CompletedAt = paymentResult.CompletedAt;
            order.Status = OrderStatus.Completed;
            
            await _context.SaveChangesAsync();

            // Step 6: Confirm ticket purchase (move from reserved to sold)
            foreach (var item in items)
            {
                await _ticketService.ConfirmTicketPurchaseAsync(item.TicketId, item.Quantity);
            }

            // Step 7: Record Double-Entry Ledger
            // This is the crucial part for financial integrity
            await _ledgerService.RecordPaymentLedgerAsync(payment.Id, totalAmount, paymentMethod);

            // Commit transaction
            await transaction.CommitAsync();

            return new CheckoutResult
            {
                Success = true,
                OrderId = order.Id,
                OrderNumber = order.OrderNumber,
                TransactionId = payment.TransactionId,
                TotalAmount = totalAmount,
                CompletedAt = payment.CompletedAt ?? DateTime.UtcNow
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            
            return new CheckoutResult
            {
                Success = false,
                ErrorMessage = $"An error occurred during checkout: {ex.Message}"
            };
        }
        });
    }

    public async Task<Order?> GetOrderByIdAsync(int orderId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Ticket)
            .FirstOrDefaultAsync(o => o.Id == orderId);
    }

    public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
    {
        return await _context.Orders
            .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Ticket)
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    private string GenerateOrderNumber()
    {
        return $"ORD-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }

    private string GenerateTransactionId()
    {
        return $"TXN-{DateTime.UtcNow:yyyyMMddHHmmss}-{Guid.NewGuid().ToString()[..8].ToUpper()}";
    }
}

public class OrderItemRequest
{
    public int TicketId { get; set; }
    public int Quantity { get; set; }
}

public class CheckoutResult
{
    public bool Success { get; set; }
    public int? OrderId { get; set; }
    public string? OrderNumber { get; set; }
    public string? TransactionId { get; set; }
    public decimal? TotalAmount { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
