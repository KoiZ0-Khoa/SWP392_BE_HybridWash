using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Storage;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HybridWash.Services.Implementations;

public class IncidentReportImageService : IIncidentReportImageService
{
    private readonly IIncidentReportRepository _incidentReportRepository;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IConfiguration _configuration;

    public IncidentReportImageService(
        IIncidentReportRepository incidentReportRepository,
        IAwsS3Service awsS3Service,
        IConfiguration configuration)
    {
        _incidentReportRepository = incidentReportRepository;
        _awsS3Service = awsS3Service;
        _configuration = configuration;
    }

    public async Task<S3FileResult> GetImageAsync(
        int reportId,
        int imageNumber,
        int requesterId,
        string requesterRole,
        CancellationToken cancellationToken = default)
    {
        if (imageNumber is not (1 or 2))
        {
            throw new ArgumentOutOfRangeException(
                nameof(imageNumber),
                "Image number must be 1 or 2.");
        }

        var report = await _incidentReportRepository.GetByIdAsync(reportId)
            ?? throw new KeyNotFoundException("Incident report not found.");

        var isAdminOrStaff = requesterRole.Equals("Admin", StringComparison.OrdinalIgnoreCase)
            || requesterRole.Equals("Staff", StringComparison.OrdinalIgnoreCase);
        var isOwner = requesterRole.Equals("Customer", StringComparison.OrdinalIgnoreCase)
            && report.CustomerId == requesterId;

        if (!isAdminOrStaff && !isOwner)
        {
            throw new UnauthorizedAccessException(
                "You do not have permission to view this incident report image.");
        }

        var fileUrlOrKey = imageNumber == 1
            ? report.ReportedImage1
            : report.ReportedImage2;
        if (string.IsNullOrWhiteSpace(fileUrlOrKey))
        {
            throw new FileNotFoundException("Incident report image not found.");
        }

        var bucketName = _configuration["AWS:BucketName"];
        if (string.IsNullOrWhiteSpace(bucketName))
        {
            throw new InvalidOperationException("AWS bucket name is not configured.");
        }

        return await _awsS3Service.DownloadFileAsync(
            fileUrlOrKey,
            bucketName,
            cancellationToken);
    }
}
