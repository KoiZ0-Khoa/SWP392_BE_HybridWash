using HybridWash.Repositories.Interfaces;
using HybridWash.Services.DTOs.Storage;
using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HybridWash.Services.Implementations;

public class BookingImageService : IBookingImageService
{
    private readonly IBookingRepository _bookingRepository;
    private readonly IAwsS3Service _awsS3Service;
    private readonly IConfiguration _configuration;

    public BookingImageService(
        IBookingRepository bookingRepository,
        IAwsS3Service awsS3Service,
        IConfiguration configuration)
    {
        _bookingRepository = bookingRepository;
        _awsS3Service = awsS3Service;
        _configuration = configuration;
    }

    public async Task<S3FileResult> GetIncidentImageAsync(
        int bookingId,
        int imageNumber,
        int requesterId,
        string requesterRole,
        CancellationToken cancellationToken = default)
    {
        if (imageNumber is not (1 or 2))
            throw new ArgumentOutOfRangeException(
                nameof(imageNumber),
                "Image number must be 1 or 2.");

        var booking = await _bookingRepository.GetBookingByIdWithDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        var isStaffOrAdmin = requesterRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
            || requesterRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        var isOwner = requesterRole.Equals("Customer", StringComparison.OrdinalIgnoreCase)
            && booking.CustomerId == requesterId;

        if (!isStaffOrAdmin && !isOwner)
            throw new UnauthorizedAccessException(
                "You do not have permission to view this booking image.");

        var fileUrlOrKey = imageNumber == 1
            ? booking.IncidentImage1
            : booking.IncidentImage2;
        if (string.IsNullOrWhiteSpace(fileUrlOrKey))
            throw new FileNotFoundException("Incident image not found.");

        var bucketName = _configuration["AWS:BucketName"];
        if (string.IsNullOrWhiteSpace(bucketName))
            throw new InvalidOperationException("AWS bucket name is not configured.");

        return await _awsS3Service.DownloadFileAsync(
            fileUrlOrKey,
            bucketName,
            cancellationToken);
    }
}
