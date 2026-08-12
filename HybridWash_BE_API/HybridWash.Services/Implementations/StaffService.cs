using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations
{
    public class StaffService : IStaffService
    {
        private readonly IStaffRepository _staffRepository;

        public StaffService(IStaffRepository staffRepository)
        {
            _staffRepository = staffRepository;
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

        public async Task<string> CheckInAsync(BookingIdRequestDTO request)
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

            if (request.IsCustomerLeaving && string.IsNullOrEmpty(request.CustomerSignature))
            {
                throw new Exception("Khách hàng rời đi nên bắt buộc phải có chữ ký xác nhận (CustomerSignature).");
            }

            // Lưu 2 ảnh hiện trạng xe (Incident Images)
            booking.IncidentImage1 = request.IncidentImage1;
            booking.IncidentImage2 = request.IncidentImage2;

            var receipt = new ParkingReceipt
            {
                BookingId = request.BookingId,
                IssueStaffId = staffId,
                Status = "Issued",
                IssuedAt = DateTime.UtcNow,
                IsCustomerLeaving = request.IsCustomerLeaving,
                CustomerSignature = request.CustomerSignature
            };

            await _staffRepository.AddParkingReceiptAsync(receipt);
            await _staffRepository.UpdateBookingAsync(booking);
            await _staffRepository.SaveChangesAsync();

            return true;
        }

        public async Task<bool> VerifyReceiptAsync(VerifyReceiptRequestDTO request, int staffId)
        {
            var receipt = await _staffRepository.GetParkingReceiptByBookingIdAsync(request.BookingId);
            if (receipt == null)
            {
                throw new Exception("Không tìm thấy Biên bản gửi xe cho Booking này.");
            }

            if (receipt.Status == "Verified")
            {
                throw new Exception("Biên bản này đã được xác nhận (Verified) trước đó.");
            }

            receipt.Status = "Verified";
            receipt.VerifyStaffId = staffId;
            receipt.VerifiedAt = DateTime.UtcNow;

            await _staffRepository.UpdateParkingReceiptAsync(receipt);
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

            booking.Status = "CheckedOut";
            
            await _staffRepository.UpdateBookingAsync(booking);
            await _staffRepository.SaveChangesAsync();

            return $"Check-out thành công cho xe có biển số {licensePlate}. Đơn hàng đã hoàn tất.";
        }
    }
}
