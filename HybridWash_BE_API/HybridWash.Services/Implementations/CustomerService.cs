using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;

namespace HybridWash.Services.Implementations
{
    public class CustomerService : ICustomerService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<VehicleResponseDTO>> GetMyVehiclesAsync(int customerId)
        {
            var vehicles = await _customerRepository.GetCustomerVehiclesAsync(customerId);
            return vehicles.Select(v => new VehicleResponseDTO
            {
                VehicleId = v.VehicleId,
                LicensePlate = v.LicensePlate,
                VehicleType = v.VehicleType
            });
        }

        public async Task<VehicleResponseDTO> AddVehicleAsync(int customerId, AddVehicleRequestDTO request)
        {
            var vehicle = new HybridWash.Entities.Models.Vehicle
            {
                CustomerId = customerId,
                LicensePlate = request.LicensePlate,
                VehicleType = request.VehicleType,
                CreatedAt = System.DateTime.UtcNow
            };

            await _customerRepository.AddVehicleAsync(vehicle);
            await _customerRepository.SaveChangesAsync();

            return new VehicleResponseDTO
            {
                VehicleId = vehicle.VehicleId,
                LicensePlate = vehicle.LicensePlate,
                VehicleType = vehicle.VehicleType
            };
        }
    }
}
