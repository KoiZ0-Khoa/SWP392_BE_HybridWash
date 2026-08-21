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
        if (imageNumber is < 1 or > 5)
            throw new ArgumentOutOfRangeException(
                nameof(imageNumber),
                "Image number must be between 1 and 5.");

        var booking = await _bookingRepository.GetBookingByIdWithDetailsAsync(bookingId)
            ?? throw new KeyNotFoundException("Booking not found.");

        var isStaffOrAdmin = requesterRole.Equals("Staff", StringComparison.OrdinalIgnoreCase)
            || requesterRole.Equals("Admin", StringComparison.OrdinalIgnoreCase);
        var isOwner = requesterRole.Equals("Customer", StringComparison.OrdinalIgnoreCase)
            && booking.CustomerId == requesterId;

        if (!isStaffOrAdmin && !isOwner)
            throw new UnauthorizedAccessException(
                "You do not have permission to view this booking image.");

        var fileUrlOrKey = imageNumber switch
        {
            1 => booking.IncidentImage1,
            2 => booking.IncidentImage2,
            3 => booking.IncidentImage3,
            4 => booking.IncidentImage4,
            5 => booking.IncidentImage5,
            _ => null
        };
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
