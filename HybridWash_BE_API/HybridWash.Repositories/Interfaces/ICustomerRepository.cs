using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces
{
    public interface ICustomerRepository
    {
        Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId);
    }
}
