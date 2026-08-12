using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.DTOs.TimeSlot
{
    public class AvailableSlotDto
    {
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int CarCapacity { get; set; }
        public int BikeCapacity { get; set; }
        public int CarBookedCount { get; set; }
        public int BikeBookedCount { get; set; }
        public int RemainingCarCapacity { get; set; }
        public int RemainingBikeCapacity { get; set; }
    }
}
