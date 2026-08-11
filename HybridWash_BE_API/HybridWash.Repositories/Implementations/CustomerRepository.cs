using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly AutowashContext _context;

        public CustomerRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Vehicle>> GetCustomerVehiclesAsync(int customerId)
        {
            return await _context.Vehicles
                .Where(v => v.CustomerId == customerId)
                .ToListAsync();
        }
    }
}
