using ETicketingSystem.Common;
using ETicketingSystem.Payment.Interfaces;

namespace ETicketingSystem.Payment.Handlers;

public class QRScanHandler : IPaymentHandler
{
    public PaymentMethod PaymentMethod => PaymentMethod.QRScan;

    public async Task<PaymentResult> ProcessPaymentAsync(Entities.Payment payment)
    {
        // Simulate QR scan processing with 8-second delay
        await Task.Delay(8000);

        return new PaymentResult
        {
            Success = true,
            CompletedAt = DateTime.UtcNow
        };
    }
}
