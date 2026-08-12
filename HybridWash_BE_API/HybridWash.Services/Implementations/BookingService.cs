using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.DTOs.Booking;
using HybridWash.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Services.Implementations
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly ITimeSlotRepository _timeSlotRepo;
        private readonly IPromotionRepository _promotionRepo;

        public BookingService(
            IBookingRepository bookingRepo,
            IServiceRepository serviceRepo,
            ITimeSlotRepository timeSlotRepo,
            IPromotionRepository promotionRepo)
        {
            _bookingRepo = bookingRepo;
            _serviceRepo = serviceRepo;
            _timeSlotRepo = timeSlotRepo;
            _promotionRepo = promotionRepo;
        }

        public async Task<BookingDto> CreateBookingAsync(CreateBookingDto dto)
        {
            Customer? customer = null;
            Vehicle? vehicle = null;
            string vehicleType;

            // --- Member hay Guest? ---
            if (dto.CustomerId.HasValue)
            {
                customer = await _bookingRepo.GetCustomerByIdAsync(dto.CustomerId.Value);
                if (customer == null)
                    throw new Exception("Customer not found");

                if (!dto.VehicleId.HasValue)
                    throw new Exception("VehicleId is required for member booking");

                vehicle = await _bookingRepo.GetVehicleByIdAsync(dto.VehicleId.Value);
                if (vehicle == null)
                    throw new Exception("Vehicle not found");
                if (vehicle.CustomerId != dto.CustomerId.Value)
                    throw new Exception("Vehicle does not belong to this customer");

                vehicleType = vehicle.VehicleType ?? "Car";
            }
            else
            {
                if (string.IsNullOrEmpty(dto.GuestName))
                    throw new Exception("GuestName is required for guest booking");
                if (string.IsNullOrEmpty(dto.GuestPhone))
                    throw new Exception("GuestPhone is required for guest booking");
                if (string.IsNullOrEmpty(dto.GuestLicensePlate))
                    throw new Exception("GuestLicensePlate is required for guest booking");

                vehicleType = dto.GuestVehicleType ?? "Car";
            }

            // --- Validate Service ---
            var service = await _serviceRepo.GetServiceByIdAsync(dto.ServiceId);
            if (service == null || service.IsActive != true)
                throw new Exception("Service not found or inactive");

            // --- Validate Slot ---
            var slot = await _timeSlotRepo.GetTimeSlotByIdAsync(dto.SlotId);
            if (slot == null || slot.IsActive != true)
                throw new Exception("Slot not found or inactive");

            // --- BookingDate >= today ---
            var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(7));
            if (dto.BookingDate < today)
                throw new Exception("Cannot book in the past");

            // --- Tier-based booking window (Member only) ---
            if (customer != null)
            {
                int maxDays = customer.CurrentTier switch
                {
                    "Member" => 7,
                    "Silver" => 10,
                    "Gold" => 12,
                    "Platinum" => 14,
                    _ => 7
                };
                if (dto.BookingDate > today.AddDays(maxDays))
                    throw new Exception(
                        $"Your tier ({customer.CurrentTier}) only allows booking up to {maxDays} days in advance");
            }

            // --- Slot capacity check (Car/Bike riêng) ---
            var bookedCount = await _bookingRepo
                .CountBookingsInSlotByTypeAsync(dto.SlotId, dto.BookingDate, vehicleType);
            int capacity = vehicleType == "Car" ? slot.CarCapacity : slot.BikeCapacity;
            if (bookedCount >= capacity)
                throw new Exception($"This slot is fully booked for {vehicleType} on the selected date");

            // --- Duplicate check ---
            var duplicate = await _bookingRepo
                .HasDuplicateBookingAsync(dto.CustomerId, dto.SlotId, dto.BookingDate, dto.GuestPhone);
            if (duplicate)
                throw new Exception("You already have a booking for this date and slot");

            var (promotionId, finalPrice) = await ApplyPromotionAsync(
                dto.PromotionId,
                dto.ServiceId,
                service.Price,
                customer);

            // --- Tạo Booking ---
            var booking = new Booking
            {
                CustomerId = dto.CustomerId,
                VehicleId = dto.VehicleId,
                GuestName = dto.GuestName,
                GuestPhone = dto.GuestPhone,
                GuestLicensePlate = dto.GuestLicensePlate,
                GuestVehicleType = dto.GuestVehicleType,
                ServiceId = dto.ServiceId,
                SlotId = dto.SlotId,
                BookingDate = dto.BookingDate,
                PromotionId = promotionId,
                OriginalPrice = service.Price,
                FinalPrice = finalPrice,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _bookingRepo.CreateBookingAsync(booking);

            return new BookingDto
            {
                BookingId = booking.BookingId,
                CustomerId = booking.CustomerId,
                CustomerName = customer?.FullName ?? dto.GuestName,
                VehicleId = booking.VehicleId,
                LicensePlate = vehicle?.LicensePlate ?? dto.GuestLicensePlate ?? "",
                VehicleType = vehicle?.VehicleType ?? dto.GuestVehicleType,
                ServiceId = service.ServiceId,
                ServiceName = service.ServiceName,
                SlotId = slot.SlotId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                BookingDate = booking.BookingDate,
                OriginalPrice = booking.OriginalPrice,
                FinalPrice = booking.FinalPrice,
                PromotionId = booking.PromotionId,
                Status = booking.Status,
                CreatedAt = booking.CreatedAt
            };
        }

        private async Task<(int? PromotionId, decimal FinalPrice)> ApplyPromotionAsync(
            int? promotionId,
            int serviceId,
            decimal originalPrice,
            Customer? customer)
        {
            if (!promotionId.HasValue)
            {
                return (null, originalPrice);
            }

            var promotion = await _promotionRepo.GetByIdAsync(promotionId.Value);
            var now = DateTime.UtcNow;
            if (promotion == null
                || !promotion.IsActive
                || (promotion.ValidFrom.HasValue && promotion.ValidFrom > now)
                || (promotion.ValidTo.HasValue && promotion.ValidTo < now))
            {
                throw new Exception("Promotion not found, inactive or expired");
            }

            var requiredTier = promotion.TargetTier ?? "All";
            if (customer == null && !requiredTier.Equals("All", StringComparison.OrdinalIgnoreCase))
            {
                throw new Exception("Guest can only use promotions for all tiers");
            }

            if (customer != null
                && !BenefitRules.IsTierEligible(customer.CurrentTier, requiredTier))
            {
                throw new Exception("Your tier is not eligible for this promotion");
            }

            var promoType = BenefitRules.NormalizeType(promotion.PromoType ?? string.Empty);
            if (promoType == "FreeWash")
            {
                if (promotion.ServiceId != serviceId)
                {
                    throw new Exception("This free-wash promotion is not valid for the selected service");
                }

                return (promotion.PromotionId, 0m);
            }

            if (promoType == "AddOn")
            {
                if (!promotion.ServiceId.HasValue)
                {
                    throw new Exception("Add-on promotion configuration is incomplete");
                }

                return (promotion.PromotionId, originalPrice);
            }

            if (promotion.ServiceId.HasValue && promotion.ServiceId != serviceId)
            {
                throw new Exception("This discount promotion is not valid for the selected service");
            }

            if (!promotion.DiscountValue.HasValue || string.IsNullOrWhiteSpace(promotion.DiscountType))
            {
                throw new Exception("Discount promotion configuration is incomplete");
            }

            var discountType = BenefitRules.NormalizeDiscountType(promotion.DiscountType);
            var discount = discountType == "Fixed"
                ? promotion.DiscountValue.Value
                : originalPrice * promotion.DiscountValue.Value / 100m;

            if (discountType == "Percent" && promotion.MaxDiscount.HasValue)
            {
                discount = Math.Min(discount, promotion.MaxDiscount.Value);
            }

            var finalPrice = Math.Max(0m, originalPrice - discount);
            return (promotion.PromotionId, decimal.Round(finalPrice, 2));
        }

        // ========== GET BY CUSTOMER ==========
        public async Task<List<BookingDto>> GetBookingsByCustomerIdAsync(int customerId)
        {
            var bookings = await _bookingRepo.GetBookingsByCustomerIdAsync(customerId);
            return bookings.Select(MapToBookingDto).ToList();
        }

        // ========== GET DETAIL ==========
        public async Task<BookingDetailDto> GetBookingByIdAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetBookingByIdWithDetailsAsync(bookingId);
            if (booking == null)
                throw new Exception("Booking not found");
            return MapToBookingDetailDto(booking);
        }

        // ========== CANCEL ==========
        public async Task CancelBookingAsync(int bookingId)
        {
            var booking = await _bookingRepo.GetBookingByIdWithDetailsAsync(bookingId);
            if (booking == null)
                throw new Exception("Booking not found");
            if (booking.Status != "Pending")
                throw new Exception($"Cannot cancel booking with status: {booking.Status}");

            booking.Status = "Cancelled";
            await _bookingRepo.SaveChangesAsync();
        }

        // ========== ADMIN LIST ==========
        public async Task<PagedResultDto<BookingDto>> GetAdminBookingsAsync(AdminBookingQueryDto q)
        {
            var query = _bookingRepo.GetBookingsQueryable();

            if (q.Date.HasValue)
                query = query.Where(b => b.BookingDate == q.Date.Value);
            if (!string.IsNullOrEmpty(q.Status))
                query = query.Where(b => b.Status == q.Status);
            if (!string.IsNullOrEmpty(q.Tier))
                query = query.Where(b => b.Customer != null && b.Customer.CurrentTier == q.Tier);
            if (!string.IsNullOrEmpty(q.VehicleType))
                query = query.Where(b =>
                    (b.Vehicle != null && b.Vehicle.VehicleType == q.VehicleType)
                    || b.GuestVehicleType == q.VehicleType);

            var desc = q.SortOrder?.ToLower() == "desc";
            query = q.SortBy?.ToLower() switch
            {
                "status" => desc ? query.OrderByDescending(b => b.Status)
                                 : query.OrderBy(b => b.Status),
                "tier" => desc ? query.OrderByDescending(b => b.Customer!.CurrentTier)
                               : query.OrderBy(b => b.Customer!.CurrentTier),
                _ => desc ? query.OrderByDescending(b => b.BookingDate)
                          : query.OrderBy(b => b.BookingDate)
            };

            var totalCount = await query.CountAsync();
            var page = q.PageNumber < 1 ? 1 : q.PageNumber;
            var size = q.PageSize < 1 ? 10 : q.PageSize;

            var items = await query
                .Skip((page - 1) * size)
                .Take(size)
                .ToListAsync();

            return new PagedResultDto<BookingDto>
            {
                TotalCount = totalCount,
                PageNumber = page,
                PageSize = size,
                Items = items.Select(MapToBookingDto).ToList()
            };
        }

        // ========== MAPPING ==========
        private static BookingDto MapToBookingDto(Booking b) => new()
        {
            BookingId = b.BookingId,
            CustomerId = b.CustomerId,
            CustomerName = b.Customer?.FullName ?? b.GuestName,
            VehicleId = b.VehicleId,
            LicensePlate = b.Vehicle?.LicensePlate ?? b.GuestLicensePlate ?? "",
            VehicleType = b.Vehicle?.VehicleType ?? b.GuestVehicleType,
            ServiceId = b.ServiceId,
            ServiceName = b.Service?.ServiceName ?? "",
            SlotId = b.SlotId,
            StartTime = b.Slot?.StartTime ?? default,
            EndTime = b.Slot?.EndTime ?? default,
            BookingDate = b.BookingDate,
            OriginalPrice = b.OriginalPrice,
            FinalPrice = b.FinalPrice,
            PromotionId = b.PromotionId,
            Status = b.Status,
            CreatedAt = b.CreatedAt
        };

        private static BookingDetailDto MapToBookingDetailDto(Booking b) => new()
        {
            BookingId = b.BookingId,
            CustomerId = b.CustomerId,
            CustomerName = b.Customer?.FullName ?? b.GuestName,
            CustomerPhone = b.Customer?.PhoneNumber ?? b.GuestPhone,
            CustomerTier = b.Customer?.CurrentTier,
            VehicleId = b.VehicleId,
            LicensePlate = b.Vehicle?.LicensePlate ?? b.GuestLicensePlate ?? "",
            VehicleType = b.Vehicle?.VehicleType ?? b.GuestVehicleType,
            ServiceId = b.ServiceId,
            ServiceName = b.Service?.ServiceName ?? "",
            ServicePrice = b.Service?.Price ?? 0,
            SlotId = b.SlotId,
            StartTime = b.Slot?.StartTime ?? default,
            EndTime = b.Slot?.EndTime ?? default,
            BookingDate = b.BookingDate,
            OriginalPrice = b.OriginalPrice,
            FinalPrice = b.FinalPrice,
            PromotionId = b.PromotionId,
            PromoCode = b.Promotion?.PromoCode,
            Status = b.Status,
            StaffId = b.StaffId,
            StaffName = b.Staff?.FullName,
            ActualWashTime = b.ActualWashTime,
            StaffNote = b.StaffNote,
            CreatedAt = b.CreatedAt
        };
    }
}
