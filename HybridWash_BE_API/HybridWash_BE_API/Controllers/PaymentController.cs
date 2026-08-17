using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using HybridWash.Services.Interfaces;
using System;

namespace HybridWash_BE_API.Controllers
{
    [Route("api/payments")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost("deposit-qr/{bookingId}")]
        public async Task<IActionResult> CreateDepositQr(int bookingId, [FromQuery] string? returnUrl = null, [FromQuery] string? cancelUrl = null)
        {
            try
            {
                var paymentInfo = await _paymentService.CreateDepositPaymentLinkAsync(bookingId, returnUrl, cancelUrl);
                return Ok(paymentInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("final-qr/{bookingId}")]
        public async Task<IActionResult> CreateFinalQr(int bookingId, [FromQuery] string? returnUrl = null, [FromQuery] string? cancelUrl = null)
        {
            try
            {
                var paymentInfo = await _paymentService.CreateFinalPaymentLinkAsync(bookingId, returnUrl, cancelUrl);
                return Ok(paymentInfo);
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> HandleWebhook([FromBody] PayOS.Models.Webhooks.Webhook webhook)
        {
            try
            {
                var success = await _paymentService.HandlePayOSWebhookAsync(webhook);
                if (success)
                    return Ok(new { success = true });
                else
                    return BadRequest(new { success = false, message = "Webhook verification failed or booking not found" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { success = false, message = ex.Message });
            }
        }
    }
}
