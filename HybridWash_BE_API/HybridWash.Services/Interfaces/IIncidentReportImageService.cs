using HybridWash.Services.DTOs.Storage;

namespace HybridWash.Services.Interfaces;

public interface IIncidentReportImageService
{
    Task<S3FileResult> GetImageAsync(
        int reportId,
        int imageNumber,
        int requesterId,
        string requesterRole,
        CancellationToken cancellationToken = default);
}
