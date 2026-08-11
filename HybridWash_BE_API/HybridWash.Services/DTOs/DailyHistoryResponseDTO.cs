using System;
using System.Collections.Generic;

namespace HybridWash.Services.DTOs
{
    public class DailyHistoryResponseDTO
    {
        public DateOnly Date { get; set; }
        
        // Tổng số xe trong ngày (bao gồm cả lịch đặt chưa đến)
        public int TotalBookings { get; set; }
        
        // Tổng số xe đã vào (Check-in/Washing/Completed/CheckedOut)
        public int VehiclesIn { get; set; }
        
        // Tổng số xe đã ra (CheckedOut)
        public int VehiclesOut { get; set; }

        public IEnumerable<BookingResponseDTO> Bookings { get; set; } = new List<BookingResponseDTO>();
    }
}
