using HybridWash.Entities.Models;
using HybridWash.Repositories.Implementations;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.TimeSlot;
using HybridWash.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.Implementations
{
    public class TimeSlotService : ITimeSlotService
    {
        private readonly ITimeSlotRepository _timeSlotRepository;
        public TimeSlotService(ITimeSlotRepository timeSlotRepository)
        {
            _timeSlotRepository = timeSlotRepository;
        }

        public async Task<TimeSlotDto> CreateTimeSlotAsync(CreateTimeSlotDto dto)
        {
            // Validate
            if (dto.StartTime >= dto.EndTime)
                throw new Exception("StartTime must be before EndTime");

            if (dto.CarCapacity < 0 || dto.BikeCapacity < 0)
                throw new Exception("Capacity cannot be negative");

            var timeSlot = new TimeSlot
            {
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                CarCapacity = dto.CarCapacity,
                BikeCapacity = dto.BikeCapacity,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };
            await _timeSlotRepository.AddTimeSlotAsync(timeSlot);
            return new TimeSlotDto
            {
                SlotId = timeSlot.SlotId,
                StartTime = timeSlot.StartTime,
                EndTime = timeSlot.EndTime,
                CarCapacity = timeSlot.CarCapacity,
                BikeCapacity = timeSlot.BikeCapacity,
                IsActive = timeSlot.IsActive
            };
        }

        public async Task<List<TimeSlotDto>> GetAllTimeSlotsAsync()
        {
            var slots = await _timeSlotRepository.GetAllTimeSlotsAsync();
            return slots.Select(s => new TimeSlotDto
            {
                SlotId = s.SlotId,
                StartTime = s.StartTime,
                EndTime = s.EndTime,
                CarCapacity = s.CarCapacity,
                BikeCapacity = s.BikeCapacity,
                IsActive = s.IsActive
            }).ToList();
        }

        public async Task<List<AvailableSlotDto>> GetAvailableSlotsAsync(DateOnly date)
        {
            var activeSlots = await _timeSlotRepository.GetActiveTimeSlotsAsync();
            var result = new List<AvailableSlotDto>();
            foreach (var slot in activeSlots)
            {
                var (carBooked, bikeBooked) = await _timeSlotRepository.CountBookingsInSlotAsync(slot.SlotId, date);

                var remainingCar = slot.CarCapacity - carBooked;
                var remainingBike = slot.BikeCapacity - bikeBooked;

                if (remainingCar > 0 || remainingBike > 0)
                {
                    result.Add(new AvailableSlotDto
                    {
                        SlotId = slot.SlotId,
                        StartTime = slot.StartTime,
                        EndTime = slot.EndTime,
                        CarCapacity = slot.CarCapacity,
                        BikeCapacity = slot.BikeCapacity,
                        CarBookedCount = carBooked,
                        BikeBookedCount = bikeBooked,
                        RemainingCarCapacity = remainingCar,
                        RemainingBikeCapacity = remainingBike
                    });
                }
            }
            return result;
        }


        public async Task<TimeSlotDto?> GetTimeSlotByIdAsync(int slotId)
        {
            var slot = await _timeSlotRepository.GetTimeSlotByIdAsync(slotId);
            if (slot == null) return null;

            return new TimeSlotDto
            {
                SlotId = slot.SlotId,
                StartTime = slot.StartTime,
                EndTime = slot.EndTime,
                CarCapacity = slot.CarCapacity,
                BikeCapacity = slot.BikeCapacity,
                IsActive = slot.IsActive
            };
        }

    }
}
