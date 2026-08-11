using System;
using System.Collections.Generic;

namespace HybridWash.Entities.Models;

public partial class TimeSlot
{
    public int SlotId { get; set; }

    public TimeOnly StartTime { get; set; }

    public TimeOnly EndTime { get; set; }

    public int CarCapacity { get; set; } = 2;

    public int BikeCapacity { get; set; } = 5;

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
