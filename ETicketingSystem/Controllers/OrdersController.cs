using ETicketingSystem.Common;
using ETicketingSystem.Ticket.Services;
using Microsoft.AspNetCore.Mvc;

namespace ETicketingSystem.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly OrderService _orderService;

    public OrdersController(OrderService orderService)
    {
        _orderService = orderService;
    }

  
    [HttpPost("checkout")]
    public async Task<IActionResult> Checkout([FromBody] CheckoutRequest request)
    {
        if (request.Items == null || !request.Items.Any())
        {
            return BadRequest(new { message = "Order must contain at least one item" });
        }

        // Default to user ID 1 (from seed data)
        const int userId = 1;

        var result = await _orderService.CreateOrderAndProcessPaymentAsync(
            userId, 
            request.Items, 
            request.PaymentMethod);

        if (!result.Success)
        {
            return BadRequest(new { message = result.ErrorMessage });
        }

        return Ok(new
        {
            success = true,
            orderId = result.OrderId,
            orderNumber = result.OrderNumber,
            transactionId = result.TransactionId,
            totalAmount = result.TotalAmount?.ToString("F2"),
            completedAt = result.CompletedAt,
            message = "Payment processed successfully"
        });
    }


    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(int id)
    {
        var order = await _orderService.GetOrderByIdAsync(id);
        
        if (order == null)
        {
            return NotFound(new { message = "Order not found" });
        }

        var response = new
        {
            order.Id,
            order.OrderNumber,
            order.UserId,
            TotalAmount = order.TotalAmount.ToString("F2"),
            order.Status,
            order.CreatedAt,
            Items = order.OrderDetails.Select(od => new
            {
                Ticket = od.Ticket.Name,
                od.Quantity,
                UnitPrice = od.UnitPrice.ToString("F2"),
                TotalPrice = od.TotalPrice.ToString("F2")
            })
        };

        return Ok(response);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetUserOrders(int userId)
    {
        var orders = await _orderService.GetOrdersByUserIdAsync(userId);

        var response = orders.Select(o => new
        {
            o.Id,
            o.OrderNumber,
            TotalAmount = o.TotalAmount.ToString("F2"),
            o.Status,
            o.CreatedAt,
            ItemCount = o.OrderDetails.Count
        });

        return Ok(response);
    }
}

public class CheckoutRequest
{
    public List<OrderItemRequest> Items { get; set; } = new();
    public PaymentMethod PaymentMethod { get; set; }
}
