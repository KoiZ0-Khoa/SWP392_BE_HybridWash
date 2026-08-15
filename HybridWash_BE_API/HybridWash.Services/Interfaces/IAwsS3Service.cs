using Microsoft.AspNetCore.Http;
using HybridWash.Services.DTOs.Storage;
using System.Threading.Tasks;

namespace HybridWash.Services.Interfaces
{
    public interface IAwsS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string bucketName, string prefix);
        Task<S3FileResult> DownloadFileAsync(
            string fileUrlOrKey,
            string bucketName,
            CancellationToken cancellationToken = default);
        Task<bool> DeleteFileAsync(string fileUrl, string bucketName);
    }
}
