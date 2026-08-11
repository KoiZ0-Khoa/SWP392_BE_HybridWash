using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class Vehicle
{
    public int VehicleId { get; set; }

    public int CustomerId { get; set; }

    public string LicensePlate { get; set; } = null!;

    public string? VehicleType { get; set; }

    public string? QrCode { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual Customer Customer { get; set; } = null!;
}
