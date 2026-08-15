using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HybridWash.Entities.Models;
using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HybridWash.Services.Implementations
{
    public class IncidentReportService : IIncidentReportService
    {
        private readonly IIncidentReportRepository _repo;
        private readonly IAwsS3Service _s3Service;
        private readonly string _bucketName;

        public IncidentReportService(IIncidentReportRepository repo, IAwsS3Service s3Service, IConfiguration config)
        {
            _repo = repo;
            _s3Service = s3Service;
            _bucketName = config["AWS:BucketName"] ?? "hybridwash-images";
        }

        public async Task<IncidentReportDto> CreateReportAsync(int customerId, CreateIncidentReportDto request)
        {
            var belongs = await _repo.BookingBelongsToCustomerAsync(request.BookingId, customerId);
            if (!belongs)
            {
                throw new Exception("Ðon hàng này không thu?c v? b?n, không th? báo cáo s? c?.");
            }

            string? url1 = null;
            string? url2 = null;

            if (request.Image1 != null)
            {
                url1 = await _s3Service.UploadFileAsync(request.Image1, _bucketName, "incident-reports");
            }
            if (request.Image2 != null)
            {
                url2 = await _s3Service.UploadFileAsync(request.Image2, _bucketName, "incident-reports");
            }

            var report = new IncidentReport
            {
                BookingId = request.BookingId,
                CustomerId = customerId,
                CustomerNote = request.CustomerNote,
                ReportedImage1 = url1,
                ReportedImage2 = url2,
                Status = "Pending"
            };

            await _repo.AddAsync(report);

            var admin = await _repo.GetFirstAdminAsync();

            return MapToDto(report, admin);
        }

        public async Task<List<IncidentReportDto>> GetMyReportsAsync(int customerId)
        {
            var reports = await _repo.GetByCustomerIdAsync(customerId);
            var admin = await _repo.GetFirstAdminAsync();
            
            return reports.Select(r => MapToDto(r, admin)).ToList();
        }

        public async Task<List<IncidentReportDto>> GetAllReportsAsync()
        {
            var reports = await _repo.GetAllAsync();
            var admin = await _repo.GetFirstAdminAsync();

            return reports.Select(r => MapToDto(r, admin)).ToList();
        }

        public async Task<IncidentReportDto> ResolveReportAsync(int reportId, ResolveIncidentReportDto request)
        {
            var report = await _repo.GetByIdAsync(reportId);
            if (report == null) throw new Exception("Không tìm th?y báo cáo.");

            report.Status = request.Status;
            report.ManagerNote = request.ManagerNote;
            
            if (request.Status == "Resolved" || request.Status == "Rejected")
            {
                report.ResolvedAt = DateTime.UtcNow;
            }

            await _repo.UpdateAsync(report);
            
            var admin = await _repo.GetFirstAdminAsync();
            return MapToDto(report, admin);
        }

        private static IncidentReportDto MapToDto(IncidentReport report, Staff? admin)
        {
            return new IncidentReportDto
            {
                ReportId = report.ReportId,
                BookingId = report.BookingId,
                CustomerId = report.CustomerId,
                CustomerName = report.Customer?.FullName,
                Image1 = report.ReportedImage1,
                Image2 = report.ReportedImage2,
                CustomerNote = report.CustomerNote,
                Status = report.Status,
                ManagerNote = report.ManagerNote,
                CreatedAt = report.CreatedAt,
                ResolvedAt = report.ResolvedAt,
                ManagerContactPhone = admin?.PhoneNumber,
                ManagerContactEmail = admin?.Email ?? "manager.hybridwash@gmail.com"
            };
        }
    }
}
