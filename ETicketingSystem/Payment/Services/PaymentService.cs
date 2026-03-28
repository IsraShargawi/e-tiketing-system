using ETicketingSystem.Common;
using ETicketingSystem.Payment.Interfaces;

namespace ETicketingSystem.Payment.Services;

public class PaymentService
{
    private readonly IEnumerable<IPaymentHandler> _paymentHandlers;

    public PaymentService(IEnumerable<IPaymentHandler> paymentHandlers)
    {
        _paymentHandlers = paymentHandlers;
    }

    public async Task<PaymentResult> ProcessPaymentAsync(Entities.Payment payment)
    {
        var handler = _paymentHandlers.FirstOrDefault(h => h.PaymentMethod == payment.Method);
        
        if (handler == null)
        {
            return new PaymentResult
            {
                Success = false,
                ErrorMessage = $"No handler found for payment method: {payment.Method}"
            };
        }

        return await handler.ProcessPaymentAsync(payment);
    }
}
