
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
        Task<List<ServiceDto>> GetActiveServicesAsync();
        Task<ServiceDto> GetServiceByIdAsync(int serviceId);
    }
}
