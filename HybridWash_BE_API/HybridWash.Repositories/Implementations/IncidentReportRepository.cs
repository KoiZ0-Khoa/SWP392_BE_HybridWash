using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HybridWash.Entities.Models;
using HybridWash.Repositories.Data;
using HybridWash.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace HybridWash.Repositories.Implementations
{
    public class IncidentReportRepository : IIncidentReportRepository
    {
        private readonly AutowashContext _context;

        public IncidentReportRepository(AutowashContext context)
        {
            _context = context;
        }

        public async Task<IncidentReport> AddAsync(IncidentReport report)
        {
            _context.IncidentReports.Add(report);
            await _context.SaveChangesAsync();
            return report;
        }

        public async Task<IncidentReport?> GetByIdAsync(int reportId)
        {
            return await _context.IncidentReports
                .Include(r => r.Booking)
                .Include(r => r.Customer)
                .FirstOrDefaultAsync(r => r.ReportId == reportId);
        }

        public async Task<List<IncidentReport>> GetByCustomerIdAsync(int customerId)
        {
            return await _context.IncidentReports
                .Include(r => r.Booking)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<IncidentReport>> GetAllAsync()
        {
            return await _context.IncidentReports
                .Include(r => r.Booking)
                .Include(r => r.Customer)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(IncidentReport report)
        {
            _context.IncidentReports.Update(report);
            await _context.SaveChangesAsync();
        }

        public async Task<Staff?> GetFirstAdminAsync()
        {
            return await _context.Staff
                .FirstOrDefaultAsync(s => s.Role == "Admin" && s.IsActive == true);
        }

        public async Task<bool> BookingBelongsToCustomerAsync(int bookingId, int customerId)
        {
            return await _context.Bookings
                .AnyAsync(b => b.BookingId == bookingId && b.CustomerId == customerId);
        }
    }
}
