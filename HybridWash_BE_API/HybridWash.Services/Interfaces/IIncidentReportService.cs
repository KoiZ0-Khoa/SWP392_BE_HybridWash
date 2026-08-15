using System.Collections.Generic;
using System.Threading.Tasks;
using HybridWash.Services.DTOs;

namespace HybridWash.Services.Interfaces
{
    public interface IIncidentReportService
    {
        Task<IncidentReportDto> CreateReportAsync(int customerId, CreateIncidentReportDto request);
        Task<List<IncidentReportDto>> GetMyReportsAsync(int customerId);
        Task<List<IncidentReportDto>> GetAllReportsAsync();
        Task<IncidentReportDto> ResolveReportAsync(int reportId, ResolveIncidentReportDto request);
    }
}
