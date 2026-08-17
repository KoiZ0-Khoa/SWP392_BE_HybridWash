using System.Threading.Tasks;
using HybridWash.Services.DTOs.Payment;
using PayOS.Models.Webhooks;

namespace HybridWash.Services.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponseDTO> CreateDepositPaymentLinkAsync(int bookingId, string? returnUrl = null, string? cancelUrl = null);
    Task<PaymentResponseDTO> CreateFinalPaymentLinkAsync(int bookingId, string? returnUrl = null, string? cancelUrl = null);
    Task<bool> HandlePayOSWebhookAsync(Webhook webhook);
}

