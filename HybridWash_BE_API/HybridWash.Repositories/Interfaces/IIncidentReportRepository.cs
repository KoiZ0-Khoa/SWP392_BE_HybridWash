using System.Collections.Generic;
using System.Threading.Tasks;
using HybridWash.Entities.Models;

namespace HybridWash.Repositories.Interfaces
{
    public interface IIncidentReportRepository
    {
        Task<IncidentReport> AddAsync(IncidentReport report);
        Task<IncidentReport?> GetByIdAsync(int reportId);
        Task<List<IncidentReport>> GetByCustomerIdAsync(int customerId);
        Task<List<IncidentReport>> GetAllAsync();
        Task UpdateAsync(IncidentReport report);
        
        // Helper to get first admin
        Task<Staff?> GetFirstAdminAsync();
        
        // Ensure booking belongs to customer
        Task<bool> BookingBelongsToCustomerAsync(int bookingId, int customerId);
    }
}
