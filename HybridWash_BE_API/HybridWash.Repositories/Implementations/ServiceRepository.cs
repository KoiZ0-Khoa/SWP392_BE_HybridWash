
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

        public async Task<IReadOnlyList<Service>> GetAllServicesAsync()
        {
            return await _context.Services
                .AsNoTracking()
                .OrderByDescending(service => service.CreatedAt)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Service>> GetActiveServicesAsync()
        {
            return await _context.Services
                .AsNoTracking()
                .Where(s => s.IsActive == true)
                .OrderBy(service => service.ServiceName)
                .ToListAsync();
        }

        public async Task<Service?> GetServiceByIdAsync(int serviceId)
        {
            return await _context.Services.FindAsync(serviceId);
        }

        public async Task AddServiceAsync(Service service)
        {
            await _context.Services.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}
