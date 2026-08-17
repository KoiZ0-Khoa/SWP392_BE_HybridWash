using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs
{
    public class CreateIncidentReportDto
    {
        [Required]
        public int BookingId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string CustomerNote { get; set; } = null!;

        public IFormFile? Image1 { get; set; }
        
        public IFormFile? Image2 { get; set; }
    }

    public class ResolveIncidentReportDto
    {
        [Required]
        [MaxLength(20)]
        public string Status { get; set; } = null!;

        [MaxLength(1000)]
        public string? ManagerNote { get; set; }
    }

    public class IncidentReportDto
    {
        public int ReportId { get; set; }
        public int BookingId { get; set; }
        public int? CustomerId { get; set; }
        public string? CustomerName { get; set; }
        public string? Image1 { get; set; }
        public string? Image2 { get; set; }
        public string? Image1ApiPath => string.IsNullOrWhiteSpace(Image1)
            ? null
            : $"/api/IncidentReport/{ReportId}/images/1";
        public string? Image2ApiPath => string.IsNullOrWhiteSpace(Image2)
            ? null
            : $"/api/IncidentReport/{ReportId}/images/2";
        public string CustomerNote { get; set; } = null!;
        public string Status { get; set; } = null!;
        public string? ManagerNote { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }

        public string? ManagerContactPhone { get; set; }
        public string? ManagerContactEmail { get; set; }
    }
}
