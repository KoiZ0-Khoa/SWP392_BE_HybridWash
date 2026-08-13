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

            using var newMemoryStream = new MemoryStream();
            await file.CopyToAsync(newMemoryStream);

            var uploadRequest = new TransferUtilityUploadRequest
            {
                InputStream = newMemoryStream,
                Key = key,
                BucketName = bucketName,
                ContentType = file.ContentType
            };

            var fileTransferUtility = new TransferUtility(_s3Client);
            await fileTransferUtility.UploadAsync(uploadRequest);

            // Construct public URL. This assumes the bucket objects are publicly accessible.
            // If they are not, you would need to generate a PreSignedURL instead.
            return $"https://{bucketName}.s3.{_s3Client.Config.RegionEndpoint.SystemName}.amazonaws.com/{key}";
        }
    }
}
