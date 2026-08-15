using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces
{
    public interface ISystemParameterRepository
    {
        Task<SystemParameter?> GetSystemParameterAsync();
        Task AddSystemParameterAsync(SystemParameter parameter);
        Task SaveChangesAsync();
    }
}
