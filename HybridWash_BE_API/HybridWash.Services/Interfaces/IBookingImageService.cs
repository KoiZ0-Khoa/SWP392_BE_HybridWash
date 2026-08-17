using HybridWash.Services.DTOs.Storage;

namespace HybridWash.Services.Interfaces;

public interface IBookingImageService
{
    Task<S3FileResult> GetIncidentImageAsync(
        int bookingId,
        int imageNumber,
        int requesterId,
        string requesterRole,
        CancellationToken cancellationToken = default);
}
