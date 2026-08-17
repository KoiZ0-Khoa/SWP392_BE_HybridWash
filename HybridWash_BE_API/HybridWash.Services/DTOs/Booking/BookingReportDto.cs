using System;
using System.Collections.Generic;

namespace HybridWash.Services.DTOs.Booking;

public class BookingReportQueryDto
{
    public DateOnly? Date { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public string? Status { get; set; }

    public string? VehicleType { get; set; }
}

public class BookingReportResponseDto
{
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int TotalBookings { get; set; }
    public int CompletedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int InProgressOrDepositedBookings { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal CompletedRevenue { get; set; }
    public decimal DepositRevenue { get; set; }
    public List<BookingDto> Bookings { get; set; } = new();
}
