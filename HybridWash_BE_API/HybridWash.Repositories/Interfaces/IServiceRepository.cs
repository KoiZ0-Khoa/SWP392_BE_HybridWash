
using HybridWash.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Repositories.Interfaces
{
    public interface IServiceRepository
    {
        Task<List<Service>> GetActiveServicesAsync();
        Task<Service?> GetServiceByIdAsync(int serviceId);
    }
}
