using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using HybridWash.Services.DTOs.Storage;
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

        public async Task<S3FileResult> DownloadFileAsync(
            string fileUrlOrKey,
            string bucketName,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(fileUrlOrKey))
                throw new ArgumentException("S3 file URL or key is required.");
            if (string.IsNullOrWhiteSpace(bucketName))
                throw new ArgumentException("S3 bucket name is required.");

            var key = ExtractObjectKey(fileUrlOrKey);

            try
            {
                using var response = await _s3Client.GetObjectAsync(
                    new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = key
                    },
                    cancellationToken);

                using var memoryStream = new MemoryStream();
                await response.ResponseStream.CopyToAsync(
                    memoryStream,
                    cancellationToken);

                return new S3FileResult(
                    memoryStream.ToArray(),
                    string.IsNullOrWhiteSpace(response.Headers.ContentType)
                        ? "application/octet-stream"
                        : response.Headers.ContentType);
            }
            catch (AmazonS3Exception ex)
                when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException("S3 object not found.", ex);
            }
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

        private static string ExtractObjectKey(string fileUrlOrKey)
        {
            var value = fileUrlOrKey.Trim();
            if (Uri.TryCreate(value, UriKind.Absolute, out var uri)
                && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
            {
                value = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
            }
            else
            {
                value = value.TrimStart('/');
            }

            return string.IsNullOrWhiteSpace(value)
                ? throw new ArgumentException("S3 object key is invalid.")
                : value;
        }
    }
}
