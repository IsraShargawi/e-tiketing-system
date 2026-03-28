using ETicketingSystem.Common;
using ETicketingSystem.Payment.Interfaces;

namespace ETicketingSystem.Payment.Handlers;

public class CreditCardHandler : IPaymentHandler
{
    public PaymentMethod PaymentMethod => PaymentMethod.CreditCard;

    public async Task<PaymentResult> ProcessPaymentAsync(Entities.Payment payment)
    {
        // Simulate credit card processing (instant success)
        await Task.CompletedTask;

        return new PaymentResult
        {
            Success = true,
            CompletedAt = DateTime.UtcNow
        };
    }
}
