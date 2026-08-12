
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
        Task<IReadOnlyList<Service>> GetAllServicesAsync();
        Task<IReadOnlyList<Service>> GetActiveServicesAsync();
        Task<Service?> GetServiceByIdAsync(int serviceId);
        Task AddServiceAsync(Service service);
        Task SaveChangesAsync();
    }
}
