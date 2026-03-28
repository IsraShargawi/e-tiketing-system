using ETicketingSystem.Common;

namespace ETicketingSystem.Payment.Entities;

public class Payment : BaseEntity
{
    public string TransactionId { get; set; } = string.Empty;
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? ErrorMessage { get; set; }
}
