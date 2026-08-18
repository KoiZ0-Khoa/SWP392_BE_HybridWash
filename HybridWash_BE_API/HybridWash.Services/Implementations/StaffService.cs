using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.DTOs.Booking;
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
            _loyaltyService = loyaltyService;
            _configuration = configuration;

        }

        public async Task<IEnumerable<BookingResponseDTO>> GetTodayBookingsAsync()
        {
            var bookings = await _staffRepository.GetTodayBookingsAsync();
            return bookings.Select(MapBookingResponse);
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
                Bookings = bookings.Select(MapBookingResponse)
            };

            return response;
        }

        public async Task<bool> ConfirmBookingAsync(BookingIdRequestDTO request)
        {
            var booking = await _staffRepository.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
                throw new Exception("Booking không tồn tại.");

            if (booking.Status != "Pending" && booking.Status != "Deposited")
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

            if (booking.Status != "Pending" && booking.Status != "Confirmed" && booking.Status != "Deposited")
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

            // Upload images to S3 if provided
            var bucketName = _configuration["AWS:BucketName"] ?? "hybridwash-images";
            if (request.IncidentImage1 != null && request.IncidentImage1.Length > 0)
            {
                booking.IncidentImage1 = await _awsS3Service.UploadFileAsync(request.IncidentImage1, bucketName, "incident-images");
            }

            if (request.IncidentImage2 != null && request.IncidentImage2.Length > 0)
            {
                booking.IncidentImage2 = await _awsS3Service.UploadFileAsync(request.IncidentImage2, bucketName, "incident-images");
            }

            if (!string.IsNullOrWhiteSpace(request.StaffNote))
            {
                booking.StaffNote = request.StaffNote;
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

        public async Task<string> CheckOutAsync(BookingIdRequestDTO request, int staffId)
        {
            var booking = await _staffRepository.GetBookingByIdAsync(request.BookingId);
            if (booking == null)
            {
                throw new Exception("Booking không tồn tại.");
            }

            var licensePlate = booking.Vehicle?.LicensePlate ?? booking.GuestLicensePlate ?? "Không xác định";

            if (booking.Status == "Pending" || booking.Status == "Confirmed" || booking.Status == "Deposited")
            {
                throw new Exception($"Lỗi: Xe mang biển số {licensePlate} chưa check-in! Bạn không thể check-out xe chưa check-in.");
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

        private static BookingResponseDTO MapBookingResponse(Booking booking)
        {
            var redemption = booking.RewardRedemptions
                .OrderByDescending(item => item.UsedAt ?? item.RedeemedAt)
                .FirstOrDefault();

            AppliedRewardDto? appliedReward = null;
            if (redemption != null)
            {
                var reward = redemption.Reward;
                appliedReward = new AppliedRewardDto
                {
                    RedemptionId = redemption.RedemptionId,
                    RewardId = redemption.RewardId,
                    RewardName = reward?.RewardName ?? string.Empty,
                    RewardType = reward?.RewardType ?? string.Empty,
                    Description = reward?.Description,
                    PointsSpent = redemption.PointsSpent,
                    DiscountValue = reward?.DiscountValue,
                    ServiceId = reward?.ServiceId,
                    ServiceName = reward?.Service?.ServiceName,
                    Status = redemption.Status,
                    RedeemedAt = redemption.RedeemedAt,
                    UsedAt = redemption.UsedAt
                };
            }

            var redemptionsById = booking.RewardRedemptions.ToDictionary(
                item => item.RedemptionId);
            var addOns = booking.BookingAddOns.Select(addOn =>
            {
                RewardRedemption? addOnRedemption = null;
                if (addOn.RedemptionId.HasValue)
                {
                    redemptionsById.TryGetValue(
                        addOn.RedemptionId.Value,
                        out addOnRedemption);
                }

                return new BookingAddOnDto
                {
                    BookingAddOnId = addOn.BookingAddOnId,
                    ServiceId = addOn.ServiceId,
                    ServiceName = addOn.Service?.ServiceName ?? string.Empty,
                    PromotionId = addOn.PromotionId,
                    RedemptionId = addOn.RedemptionId,
                    RewardId = addOnRedemption?.RewardId,
                    RewardName = addOnRedemption?.Reward?.RewardName,
                    RewardType = addOnRedemption?.Reward?.RewardType,
                    OriginalPrice = addOn.OriginalPrice,
                    FinalPrice = addOn.FinalPrice,
                    Status = addOn.Status
                };
            }).ToList();

            return new BookingResponseDTO
            {
                BookingId = booking.BookingId,
                CustomerName = booking.Customer?.FullName ?? booking.GuestName,
                CustomerPhone = booking.Customer?.PhoneNumber ?? booking.GuestPhone,
                LicensePlate = booking.Vehicle?.LicensePlate
                    ?? booking.GuestLicensePlate
                    ?? string.Empty,
                VehicleType = booking.Vehicle?.VehicleType
                    ?? booking.GuestVehicleType
                    ?? string.Empty,
                Status = booking.Status ?? string.Empty,
                SlotId = booking.SlotId,
                ServiceId = booking.ServiceId,
                ServiceName = booking.Service?.ServiceName,
                BookingDate = booking.BookingDate,
                OriginalPrice = booking.OriginalPrice,
                FinalPrice = booking.FinalPrice,
                PromotionId = booking.PromotionId,
                PromoCode = booking.Promotion?.PromoCode,
                RedemptionId = redemption?.RedemptionId,
                AppliedReward = appliedReward,
                AddOns = addOns
            };
        }
    }
}
