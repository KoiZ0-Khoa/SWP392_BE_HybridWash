using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace HybridWash.Services.Interfaces
{
    public interface IAwsS3Service
    {
        Task<string> UploadFileAsync(IFormFile file, string bucketName, string prefix);
        Task<bool> DeleteFileAsync(string fileUrl, string bucketName);
    }
}
