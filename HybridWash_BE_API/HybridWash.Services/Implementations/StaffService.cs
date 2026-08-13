using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;
        private readonly IAwsS3Service _awsS3Service;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _configuration;
        private readonly ILoyaltyService _loyaltyService;

        public StaffService(IStaffRepository staffRepository, IAwsS3Service awsS3Service, ILoyaltyService loyaltyService, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            _staffRepository = staffRepository;
            _awsS3Service = awsS3Service;
            _loyalty_service = loyaltyService;
            _configuration = configuration;

        }

        public async Task<IEnumerable<BookingResponseDTO>> GetTodayBookingsAsync()
        {
            var bookings = await _staffRepository.GetTodayBookingsAsync();
            return bookings.Select(b => new BookingResponseDTO
            {
                BookingId = b.BookingId,
                CustomerName = b.Customer?.FullName ?? b.GuestName,
                CustomerPhone = b.Customer?.PhoneNumber ?? b.GuestPhone,
                LicensePlate = b.Vehicle?.LicensePlate ?? b.GuestLicensePlate ?? "",
                VehicleType = b.Vehicle?.VehicleType ?? b.GuestVehicleType,
                Status = b.Status,
                SlotId = b.SlotId,
                ServiceId = b.ServiceId,
                BookingDate = b.BookingDate
            });
        }

        public async Task<DailyHistoryResponseDTO> GetDailyHistoryAsync(DateOnly date)
        {
            var bookings = await _staffRepository.GetBookingsByDateAsync(date);

            var inStatuses = new[] { "CheckedIn", "Washing", "Completed", "CheckedOut" };

            var response = new DailyHistoryResponseDTO
            {
                Date = date,
                TotalBookings = bookings.Count(),
                VehiclesIn = bookings.Count(b => inStatuses.Contains(b.Status)),
                VehiclesOut = bookings.Count(b => b.Status == "CheckedOut"),
                Bookings = bookings.Select(b => new BookingResponseDTO
                {
                    BookingId = b.BookingId,
                    CustomerName = b.Customer?.FullName ?? b.GuestName,
                    CustomerPhone = b.Customer?.PhoneNumber ?? b.GuestPhone,
                    LicensePlate = b.Vehicle?.LicensePlate ?? b.GuestLicensePlate ?? "",
                    VehicleType = b.Vehicle?.VehicleType ?? b.GuestVehicleType,
                    Status = b.Status,
                    SlotId = b.SlotId,
                    ServiceId = b.ServiceId,
                    BookingDate = b.BookingDate
                })
            };

            return response;
        }

        public async Task<bool> ConfirmBookingAsync(BookingIdRequestDTO request)
        {
            var booking = await _staffRepository.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
                throw new Exception("Booking không tồn tại.");

            if (booking.Status != "Pending")
                throw new Exception($"Không thể Confirm Booking đang ở trạng thái: {booking.Status}");

            booking.Status = "Confirmed";
            await _staffRepository.UpdateBookingAsync(booking);
            await _staffRepository.SaveChangesAsync();

            return true;
        }

        public async Task<string> CheckInAsync(CheckInRequestDTO request)
        {
            var booking = await _staffRepository.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new Exception("Booking không tồn tại.");
            }

            var licensePlate = booking.Vehicle?.LicensePlate ?? booking.GuestLicensePlate ?? "Không xác định";

            if (booking.Status != "Pending" && booking.Status != "Confirmed")
            {
                throw new Exception($"Booking của xe {licensePlate} đang ở trạng thái {booking.Status}, không thể Check-in.");
            }

            var currentTime = DateTime.UtcNow.AddHours(7); // VN Time
            var currentDate = DateOnly.FromDateTime(currentTime);
            var currentTimeOnly = TimeOnly.FromDateTime(currentTime);

            if (booking.BookingDate != currentDate)
            {
                throw new Exception($"Booking này dành cho ngày {booking.BookingDate}, không thể Check-in hôm nay.");
            }

            var slotStartTime = booking.Slot.StartTime;
            
            if (currentTimeOnly < slotStartTime.AddMinutes(-15))
            {
                throw new Exception($"Chưa đến giờ. Vui lòng quay lại sau. (Giờ đặt: {slotStartTime})");
            }
            
            if (currentTimeOnly >= slotStartTime.AddMinutes(-15) && currentTimeOnly < slotStartTime)
            {
                var vehicleType = booking.Vehicle?.VehicleType ?? booking.GuestVehicleType ?? "Car";
                var capacity = vehicleType.Equals("Car", StringComparison.OrdinalIgnoreCase) ? booking.Slot.CarCapacity : booking.Slot.BikeCapacity;
                var activeWashings = await _staffRepository.GetActiveWashingsCountAsync(vehicleType);

                if (activeWashings >= capacity)
                {
                    throw new Exception("Vui lòng đợi đến đúng giờ, hiện không có chỗ trống.");
                }
            }

            // Upload images to S3
            var bucketName = _configuration["AWS:BucketName"] ?? "hybridwash-images";
            var image1Url = await _awsS3Service.UploadFileAsync(request.IncidentImage1, bucketName, "incident-images");
            var image2Url = await _awsS3Service.UploadFileAsync(request.IncidentImage2, bucketName, "incident-images");

            booking.IncidentImage1 = image1Url;
            booking.IncidentImage2 = image2Url;
            booking.StaffNote = request.StaffNote;

            booking.Status = "Washing";
            booking.ActualWashTime = DateTime.UtcNow;
            await _staffRepository.UpdateBookingAsync(booking);
            await _staffRepository.SaveChangesAsync();

            return $"Check-in thành công. Hệ thống đã ghi nhận xe biển số {licensePlate} vào bãi và bắt đầu rửa.";
        }

        public async Task<bool> IssueReceiptAsync(IssueReceiptRequestDTO request, int staffId)
        {
            var booking = await _staffRepository.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new Exception("Booking không tồn tại.");
            }

            var receipt = new ParkingReceipt
            {
                BookingId = request.BookingId,
                IssueStaffId = staffId,
                Status = "Issued",
                IssuedAt = DateTime.UtcNow,
                IsCustomerLeaving = request.IsCustomerLeaving
            };

            await _staffRepository.AddParkingReceiptAsync(receipt);
            await _staffRepository.UpdateBookingAsync(booking);
            await _staffRepository.SaveChangesAsync();

            return true;
        }

        public async Task<string> CheckOutAsync(BookingIdRequestDTO request, int staffId)
        {
            var booking = await _staffRepository.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new Exception("Booking không tồn tại.");
            }

            var licensePlate = booking.Vehicle?.LicensePlate ?? booking.GuestLicensePlate ?? "Không xác định";

            if (booking.Status == "Pending" || booking.Status == "Confirmed")
            {
                throw new Exception($"Lỗi: Xe mang biển số {licensePlate} chưa check-in! Bạn không thể check-out xe chưa check-in.");
            }

            var receipt = await _staffRepository.GetParkingReceiptByBookingIdAsync(booking.BookingId);
            if (receipt != null && receipt.Status != "Verified")
            {
                throw new Exception($"Biên bản gửi xe chưa được xác nhận (Verify). Vui lòng xác nhận trước khi giao xe (Check-out) cho biển số {licensePlate}.");
            }

            await _loyaltyService.CompleteBookingAndEarnPointsAsync(
                booking.BookingId,
                DateTime.UtcNow);

            booking.Status = "CheckedOut";
            foreach (var addOn in booking.BookingAddOns)
            {
                addOn.Status = "Completed";
            }
            
            await _staffRepository.UpdateBookingAsync(booking);
            await _staffRepository.SaveChangesAsync();

            return $"Check-out thành công cho xe có biển số {licensePlate}. Đơn hàng đã hoàn tất.";
        }
    }
}
