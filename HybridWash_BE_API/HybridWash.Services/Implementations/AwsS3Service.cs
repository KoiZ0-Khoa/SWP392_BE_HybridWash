using Amazon.S3;
using Amazon.S3.Transfer;
using HybridWash.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace HybridWash.Services.Implementations
{
    public class AwsS3Service : IAwsS3Service
    {
        private readonly IAmazonS3 _s3Client;

        public AwsS3Service(IAmazonS3 s3Client)
        {
            _s3Client = s3Client;
        }

        public async Task<string> UploadFileAsync(IFormFile file, string bucketName, string prefix)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty.");

            var fileExtension = Path.GetExtension(file.FileName);
            var key = $"{prefix}/{Guid.NewGuid()}{fileExtension}";

            using var stream = file.OpenReadStream();

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = stream,
                Key = key,
                BucketName = bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            // Construct public URL. This assumes the bucket objects are publicly accessible.
            var region = _s3Client.Config.RegionEndpoint?.SystemName ?? "ap-southeast-1";
            return $"https://{bucketName}.s3.{region}.amazonaws.com/{key}";
        }

        public async Task<bool> DeleteFileAsync(string fileUrl, string bucketName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(fileUrl)) return false;
                
                var uri = new Uri(fileUrl);
                // URL structure: https://{bucketName}.s3.{region}.amazonaws.com/{key}
                var key = uri.AbsolutePath.TrimStart('/');

                var deleteRequest = new Amazon.S3.Model.DeleteObjectRequest
                {
                    BucketName = bucketName,
                    Key = key
                };

                var response = await _s3Client.DeleteObjectAsync(deleteRequest);
                return response.HttpStatusCode == System.Net.HttpStatusCode.NoContent || 
                       response.HttpStatusCode == System.Net.HttpStatusCode.OK;
            }
            catch (Exception)
            {
                // Log exception if needed
                return false;
            }
        }
    }
}
