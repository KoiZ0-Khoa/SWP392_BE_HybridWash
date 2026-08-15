using System;

namespace HybridWash.Services.DTOs.Booking
{
    public class ParkingReceiptDto
    {
        public int ReceiptId { get; set; }
        public int BookingId { get; set; }
        public int? IssueStaffId { get; set; }
        public string? IssueStaffName { get; set; }
        public string Status { get; set; } = null!;
        public DateTime? IssuedAt { get; set; }
        public bool IsCustomerLeaving { get; set; }
        public string? CustomerSignature { get; set; }
    }
}
