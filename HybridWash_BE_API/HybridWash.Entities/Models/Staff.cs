using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public string PhoneNumber { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string? Email { get; set; }

    public string? Role { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<ParkingReceipt> ParkingReceiptIssueStaffs { get; set; } = new List<ParkingReceipt>();

    public virtual ICollection<ParkingReceipt> ParkingReceiptVerifyStaffs { get; set; } = new List<ParkingReceipt>();
}
