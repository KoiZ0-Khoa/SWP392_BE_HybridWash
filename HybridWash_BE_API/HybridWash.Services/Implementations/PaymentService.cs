using System;
using System.Linq;
using System.Threading.Tasks;
using HybridWash.Repositories.Data;
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
    private readonly ILoyaltyService _loyaltyService;

    public PaymentService(AutowashContext context, IConfiguration configuration, ILoyaltyService loyaltyService)
    {
        _context = context;
        _loyaltyService = loyaltyService;

        // Initialize PayOS
        var clientId = configuration["PayOS:ClientId"];
        var apiKey = configuration["PayOS:ApiKey"];
        var checksumKey = configuration["PayOS:ChecksumKey"];

        _payOS = new PayOSClient(clientId, apiKey, checksumKey);
    }

    public async Task<string> CreateDepositPaymentLinkAsync(int bookingId, string returnUrl, string cancelUrl)
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

        decimal depositAmount = 0;

        string vehicleType = booking.Vehicle?.VehicleType ?? "Bike";

        if (vehicleType.Equals("Car", StringComparison.OrdinalIgnoreCase))
        {
            depositAmount = (booking.OriginalPrice ?? 0) * (systemParam.CarDepositPercentage / 100);
        }
        else
        {
            depositAmount = systemParam.BikeDepositAmount;
        }

        booking.DepositAmount = depositAmount;
        await _context.SaveChangesAsync();

        // Create PayOS payment link
        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff")) + bookingId; // Generate unique order code

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (int)depositAmount,
            Description = $"Deposit for booking {bookingId}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

        return paymentLink.CheckoutUrl;
    }

    public async Task<string> CreateFinalPaymentLinkAsync(int bookingId, string returnUrl, string cancelUrl)
    {
        var booking = await _context.Bookings
            .FirstOrDefaultAsync(b => b.BookingId == bookingId);

        if (booking == null)
        {
            throw new Exception("Booking not found");
        }

        if (booking.Status == "Completed" || booking.Status == "CheckedOut")
        {
            throw new Exception("Booking is already completed and paid");
        }

        if (booking.Status == "Cancelled" || booking.Status == "NoShow" || booking.Status == "RefundPending")
        {
            throw new Exception($"Cannot create payment for booking with status: {booking.Status}");
        }

        decimal total = booking.FinalPrice ?? booking.OriginalPrice ?? 0;
        decimal deposit = booking.DepositAmount ?? 0;
        decimal amountToPay = total - deposit;

        if (amountToPay <= 0)
        {
            throw new Exception("No remaining balance to pay for this booking");
        }

        int orderCode = int.Parse(DateTimeOffset.Now.ToString("ffffff")) + bookingId;

        var paymentRequest = new CreatePaymentLinkRequest
        {
            OrderCode = orderCode,
            Amount = (int)amountToPay,
            Description = $"Final for booking {bookingId}",
            CancelUrl = cancelUrl,
            ReturnUrl = returnUrl
        };

        var paymentLink = await _payOS.PaymentRequests.CreateAsync(paymentRequest);

        return paymentLink.CheckoutUrl;
    }

    public async Task<bool> HandlePayOSWebhookAsync(Webhook webhook)
    {
        try
        {
            Console.WriteLine($"[PayOS Webhook] Received webhook: OrderCode={webhook.Data?.OrderCode}, Desc={webhook.Data?.Description}");

            var verifiedData = await _payOS.Webhooks.VerifyAsync(webhook);

            Console.WriteLine($"[PayOS Webhook] Verified successfully: Code={verifiedData.Code}, Desc={verifiedData.Description}");

            if (verifiedData != null && verifiedData.Code == "00")
            {
                var desc = verifiedData.Description;
                if (!string.IsNullOrEmpty(desc))
                {
                    if (desc.StartsWith("Deposit for booking ", StringComparison.OrdinalIgnoreCase))
                    {
                        string bookingIdStr = desc.Substring("Deposit for booking ".Length).Trim();
                        if (int.TryParse(bookingIdStr, out int bookingId))
                        {
                            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
                            if (booking != null && booking.Status == "Pending")
                            {
                                booking.Status = "Deposited";
                                await _context.SaveChangesAsync();
                                Console.WriteLine($"[PayOS Webhook] Updated Booking #{bookingId} to Deposited");
                            }
                            else
                            {
                                Console.WriteLine($"[PayOS Webhook] Booking #{bookingId} not found or status not Pending (current: {booking?.Status})");
                            }
                        }
                    }
                    else if (desc.StartsWith("Final for booking ", StringComparison.OrdinalIgnoreCase))
                    {
                        string bookingIdStr = desc.Substring("Final for booking ".Length).Trim();
                        if (int.TryParse(bookingIdStr, out int bookingId))
                        {
                            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.BookingId == bookingId);
                            if (booking != null && booking.Status != "Completed" && booking.Status != "CheckedOut")
                            {
                                await _loyaltyService.CompleteBookingAndEarnPointsAsync(bookingId, DateTime.UtcNow);
                                Console.WriteLine($"[PayOS Webhook] Completed Booking #{bookingId} and earned points");
                            }
                        }
                    }
                }

                return true;
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[PayOS Webhook Error] {ex.Message} \n {ex.StackTrace}");
            return false;
        }
    }
}
