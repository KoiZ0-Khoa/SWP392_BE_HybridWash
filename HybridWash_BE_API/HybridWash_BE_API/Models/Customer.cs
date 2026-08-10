using System;
using System.Collections.Generic;

namespace HybridWash_BE_API.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string PasswordHash { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? CurrentTier { get; set; }

    public decimal? TotalSpent { get; set; }

    public int? CurrentPoints { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
