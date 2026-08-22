using System;
using System.Threading.Tasks;
using HybridWash.Repositories.Data;
using HybridWash.Services.DTOs.Payment;
using HybridWash.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PayOS;
using PayOS.Models;
using PayOS.Models.V2.PaymentRequests;
using PayOS.Models.Webhooks;

namespace HybridWash.Services.Implementations;

public class PaymentService : IPaymentService
{
    private readonly AutowashContext _context;
    private readonly PayOSClient _payOS;

    public PaymentService(AutowashContext context, IConfiguration configuration)
    {
        _context = context;

        // Initialize PayOS
        var clientId = configuration["PayOS:ClientId"];
        var apiKey = configuration["PayOS:ApiKey"];
        var checksumKey = configuration["PayOS:ChecksumKey"];

        _payOS = new PayOSClient(clientId, apiKey, checksumKey);
    }

    public async Task<PaymentResponseDTO> CreateDepositPaymentLinkAsync(int bookingId, string? returnUrl = null, string? cancelUrl = null)
    {
        var booking = await _context.Bookings
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            throw new Exception("Booking not found");
        }

        if (!booking.CustomerId.HasValue)
        {
            throw new Exception("Walk-in guest bookings do not require advance deposit payment");
        }

        if (booking.Status == "Deposited")
        {
            throw new Exception("This booking has already been deposited");
        }

        if (booking.Status != "Pending")
        {
            throw new Exception($"Cannot create deposit link for booking with status: {booking.Status}");
        }

        var systemParam = await _context.SystemParameters.FirstOrDefaultAsync(x => x.Id == 1);
        if (systemParam == null)
        {
            throw new Exception("System parameters not configured");
        }

        decimal finalPrice = booking.FinalPrice ?? booking.OriginalPrice ?? 0;
        decimal depositAmount = 0;

        string vehicleType = booking.Vehicle?.VehicleType ?? booking.GuestVehicleType ?? "Bike";

        if (vehicleType.Equals("Car", StringComparison.OrdinalIgnoreCase))
        {
            depositAmount = finalPrice * (systemParam.CarDepositPercentage / 100);
        }
        else
        {
            depositAmount = finalPrice == 0 ? 0 : Math.Min(systemParam.BikeDepositAmount, finalPrice);
        }

        depositAmount = decimal.Round(depositAmount, 0, MidpointRounding.AwayFromZero);

  
        if (depositAmount <= 0)
        {
            booking.DepositAmount = 0;
            booking.Status = "Deposited";
            booking.PaymentStatus = "Paid";
            await _context.SaveChangesAsync();

            return new PaymentResponseDTO
            {
                OrderCode = 0,
                Amount = 0,
                Description = $"Deposit for booking {bookingId} (Free 0đ - Auto Deposited)",
                AccountNumber = "",
                AccountName = "",
                Bin = "",
                QrCode = "",
                CheckoutUrl = "",
                PaymentLinkId = null,
                Status = "PAID",
                QrImageUrl = ""
            };
        }

        booking.DepositAmount = depositAmount;
        await _context.SaveChangesAsync();

        // Create PayOS payment link
        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff")) + bookingId; // Generate unique order code

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = decimal.ToInt32(depositAmount),
            Description = $"Deposit for booking {bookingId}",
            CancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? "https://hybridwash.vn/payment-cancel" : cancelUrl,
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "https://hybridwash.vn/payment-success" : returnUrl
        };

        var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

        if (booking.PaymentStatus == "Failed")
        {
            booking.PaymentStatus = "Unpaid";
            await _context.SaveChangesAsync();
        }

        string qrImageUrl = !string.IsNullOrEmpty(paymentLink.Bin) && !string.IsNullOrEmpty(paymentLink.AccountNumber)
            ? $"https://img.vietqr.io/image/{paymentLink.Bin}-{paymentLink.AccountNumber}-compact2.png?amount={paymentLink.Amount}&addInfo={Uri.EscapeDataString(paymentLink.Description ?? "")}&accountName={Uri.EscapeDataString(paymentLink.AccountName ?? "")}"
            : "";

        return new PaymentResponseDTO
        {
            OrderCode = paymentLink.OrderCode,
            Amount = paymentLink.Amount,
            Description = paymentLink.Description ?? paymentRequest.Description,
            AccountNumber = paymentLink.AccountNumber ?? "",
            AccountName = paymentLink.AccountName ?? "",
            Bin = paymentLink.Bin ?? "",
            QrCode = paymentLink.QrCode ?? "",
            CheckoutUrl = paymentLink.CheckoutUrl ?? "",
            PaymentLinkId = paymentLink.PaymentLinkId,
            Status = paymentLink.Status.ToString(),
            QrImageUrl = qrImageUrl
        };
    }

    public async Task<PaymentResponseDTO> CreateFinalPaymentLinkAsync(int bookingId, string? returnUrl = null, string? cancelUrl = null)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            throw new Exception("Booking not found");
        }

        if (booking.Status != "Washing")
        {
            throw new Exception(
                $"Final payment can only be created while the booking is Washing. Current status: {booking.Status}");
        }

        if (booking.PaymentStatus == "Paid")
        {
            throw new Exception("This booking has already been fully paid");
        }

        decimal total = booking.FinalPrice ?? booking.OriginalPrice ?? 0;
        decimal deposit = booking.DepositAmount ?? 0;
        decimal amountToPay = decimal.Round(
            total - deposit,
            0,
            MidpointRounding.AwayFromZero);

        if (amountToPay <= 0)
        {
            throw new Exception("No remaining balance to pay for this booking");
        }

        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff")) + bookingId;

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = decimal.ToInt32(amountToPay),
            Description = $"Final for booking {bookingId}",
            CancelUrl = string.IsNullOrWhiteSpace(cancelUrl) ? "https://hybridwash.vn/payment-cancel" : cancelUrl,
            ReturnUrl = string.IsNullOrWhiteSpace(returnUrl) ? "https://hybridwash.vn/payment-success" : returnUrl
        };

        var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

        if (booking.PaymentStatus == "Failed")
        {
            booking.PaymentStatus = deposit > 0 ? "PartiallyPaid" : "Unpaid";
            await _context.SaveChangesAsync();
        }

        string qrImageUrl = !string.IsNullOrEmpty(paymentLink.Bin) && !string.IsNullOrEmpty(paymentLink.AccountNumber)
            ? $"https://img.vietqr.io/image/{paymentLink.Bin}-{paymentLink.AccountNumber}-compact2.png?amount={paymentLink.Amount}&addInfo={Uri.EscapeDataString(paymentLink.Description ?? "")}&accountName={Uri.EscapeDataString(paymentLink.AccountName ?? "")}"
            : "";

        return new PaymentResponseDTO
        {
            OrderCode = paymentLink.OrderCode,
            Amount = paymentLink.Amount,
            Description = paymentLink.Description ?? paymentRequest.Description,
            AccountNumber = paymentLink.AccountNumber ?? "",
            AccountName = paymentLink.AccountName ?? "",
            Bin = paymentLink.Bin ?? "",
            QrCode = paymentLink.QrCode ?? "",
            CheckoutUrl = paymentLink.CheckoutUrl ?? "",
            PaymentLinkId = paymentLink.PaymentLinkId,
            Status = paymentLink.Status.ToString(),
            QrImageUrl = qrImageUrl
        };
    }

    public async Task<bool> HandlePayOSWebhookAsync(Webhook webhook)
    {
        try
        {
            Console.WriteLine($"[PayOS Webhook] Received webhook: OrderCode={webhook.Data?.OrderCode}, Desc={webhook.Data?.Description}");

            var verifiedData = await _payOS.Webhooks.VerifyAsync(webhook);

            if (verifiedData == null)
            {
                return false;
            }

            Console.WriteLine($"[PayOS Webhook] Verified successfully: Code={verifiedData.Code}, Desc={verifiedData.Description}");

            var desc = verifiedData.Description;
            if (string.IsNullOrWhiteSpace(desc))
            {
                Console.WriteLine("[PayOS Webhook] Description is missing; webhook acknowledged without updating a booking");
                return true;
            }

            var isDeposit = desc.StartsWith("Deposit for booking ", StringComparison.OrdinalIgnoreCase);
            var isFinal = desc.StartsWith("Final for booking ", StringComparison.OrdinalIgnoreCase);
            if (!isDeposit && !isFinal)
            {
                Console.WriteLine($"[PayOS Webhook] Unknown description: {desc}");
                return true;
            }

            var prefix = isDeposit ? "Deposit for booking " : "Final for booking ";
            var bookingIdText = desc[prefix.Length..].Trim();
            if (!int.TryParse(bookingIdText, out var bookingId))
            {
                Console.WriteLine($"[PayOS Webhook] Invalid booking reference: {desc}");
                return true;
            }

            var booking = await _context.Bookings
                .FirstOrDefaultAsync(item => item.BookingId == bookingId);
            if (booking == null)
            {
                Console.WriteLine($"[PayOS Webhook] Booking #{bookingId} not found");
                return false;
            }

            if (verifiedData.Code != "00")
            {
                if (booking.PaymentStatus != "Paid")
                {
                    booking.PaymentStatus = "Failed";
                    await _context.SaveChangesAsync();
                }

                return true;
            }

            var total = booking.FinalPrice ?? booking.OriginalPrice ?? 0;
            var deposit = booking.DepositAmount ?? 0;
            var expectedAmount = isDeposit
                ? decimal.Round(deposit, 0, MidpointRounding.AwayFromZero)
                : decimal.Round(
                    Math.Max(total - deposit, 0),
                    0,
                    MidpointRounding.AwayFromZero);

            if (verifiedData.Amount != expectedAmount)
            {
                throw new InvalidOperationException(
                    $"Payment amount mismatch for booking #{bookingId}. Expected {expectedAmount}, received {verifiedData.Amount}.");
            }

            if (isDeposit)
            {
                booking.PaymentStatus = total - deposit <= 0
                    ? "Paid"
                    : "PartiallyPaid";

                if (booking.Status == "Pending")
                {
                    booking.Status = "Deposited";
                }
            }
            else
            {
                booking.PaymentStatus = "Paid";
            }

            await _context.SaveChangesAsync();
            Console.WriteLine(
                $"[PayOS Webhook] Updated Booking #{bookingId}: Status={booking.Status}, PaymentStatus={booking.PaymentStatus}");
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayOS Webhook Error] {ex.Message} \n {ex.StackTrace}");
            return false;
        }
    }
}
