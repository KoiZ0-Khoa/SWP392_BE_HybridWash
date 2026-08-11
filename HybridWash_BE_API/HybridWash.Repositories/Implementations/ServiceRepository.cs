
using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HybridWash.Repositories.Implementations
{
    public class ServiceRepository : IServiceRepository
    {
        private readonly AutowashContext _context;

        public ServiceRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task<List<Service>> GetActiveServicesAsync()
        {
            return await _context.Services
                .Where(s => s.IsActive == true)
                .ToListAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }
    }
}
