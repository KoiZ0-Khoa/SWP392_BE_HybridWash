using System;

namespace HybridWash.Entities.Models;

public partial class ParkingReceipt
{
    public int ReceiptId { get; set; }

    public int BookingId { get; set; }

    public int IssueStaffId { get; set; }

    public int? VerifyStaffId { get; set; }

    public string? Status { get; set; }

    public bool? IsCustomerLeaving { get; set; }

    public string? CustomerSignature { get; set; }

    public DateTime? IssuedAt { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual Staff IssueStaff { get; set; } = null!;

    public virtual Staff? VerifyStaff { get; set; }
}
