using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.DTOs.TimeSlot
{
    public class TimeSlotDto
    {
        public int SlotId { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public int CarCapacity { get; set; }
        public int BikeCapacity { get; set; }
        public bool? IsActive { get; set; }
    }
}
