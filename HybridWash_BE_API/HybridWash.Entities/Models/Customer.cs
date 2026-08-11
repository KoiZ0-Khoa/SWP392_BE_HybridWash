using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class Customer
{
    public int CustomerId { get; set; }

    public string PhoneNumber { get; set; } = null!;

    public string FullName { get; set; } = null!;

    public string? CurrentTier { get; set; }

    public decimal? TotalSpent { get; set; }

    public int? CurrentPoints { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? PasswordHash { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();

    public virtual ICollection<RewardRedemption> RewardRedemptions { get; set; } = new List<RewardRedemption>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
