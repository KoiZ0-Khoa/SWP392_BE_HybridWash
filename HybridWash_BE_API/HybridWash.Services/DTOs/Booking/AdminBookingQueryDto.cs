using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.DTOs.Booking
{
    public class AdminBookingQueryDto
    {
        public DateOnly? Date { get; set; }
        public string? Status { get; set; }
        public string? Tier { get; set; }
        public string? VehicleType { get; set; } 
        public string? SortBy { get; set; }         // "date", "status", "tier"
        public string? SortOrder { get; set; }      // "asc" or "desc"
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
