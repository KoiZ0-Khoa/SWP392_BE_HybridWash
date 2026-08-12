
using HybridWash.Services.DTOs.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Services.Interfaces
{
    public interface IServiceService
    {
        Task<IReadOnlyList<ServiceDto>> GetAllServicesAsync();
        Task<IReadOnlyList<ServiceDto>> GetActiveServicesAsync();
        Task<ServiceDto?> GetServiceByIdAsync(int serviceId);
        Task<ServiceDto> CreateServiceAsync(UpsertServiceDto request);
        Task<bool> UpdateServiceAsync(int serviceId, UpsertServiceDto request);
        Task<bool> DeactivateServiceAsync(int serviceId);
    }
}
