using ETicketingSystem.Common;
using ETicketingSystem.Payment.Entities;

namespace ETicketingSystem.Payment.Interfaces;

public interface IPaymentHandler
{
    PaymentMethod PaymentMethod { get; }
    Task<PaymentResult> ProcessPaymentAsync(Entities.Payment payment);
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime? CompletedAt { get; set; }
}
