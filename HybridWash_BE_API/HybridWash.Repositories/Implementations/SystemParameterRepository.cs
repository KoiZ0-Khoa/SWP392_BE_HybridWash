using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations
{
    public class SystemParameterRepository : ISystemParameterRepository
    {
        private readonly AutowashContext _context;

        public SystemParameterRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task<SystemParameter?> GetSystemParameterAsync()
        {
            return await _context.SystemParameters.FirstOrDefaultAsync(x => x.Id == 1);
        }

        public async Task AddSystemParameterAsync(SystemParameter parameter)
        {
            await _context.SystemParameters.AddAsync(parameter);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
