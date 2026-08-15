using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface ICustomerService
    {
        Task<IEnumerable<VehicleResponseDTO>> GetMyVehiclesAsync(int customerId);
        Task<VehicleResponseDTO> AddVehicleAsync(int customerId, AddVehicleRequestDTO request);
    }
}
