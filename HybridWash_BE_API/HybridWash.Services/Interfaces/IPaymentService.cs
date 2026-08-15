using System.Threading.Tasks;
using PayOS.Models.Webhooks;

namespace HybridWash.Services.Interfaces;

public interface IPaymentService
{
    Task<string> CreateDepositPaymentLinkAsync(int bookingId, string returnUrl, string cancelUrl);
    Task<string> CreateFinalPaymentLinkAsync(int bookingId, string returnUrl, string cancelUrl);
    Task<bool> HandlePayOSWebhookAsync(Webhook webhook);
}
