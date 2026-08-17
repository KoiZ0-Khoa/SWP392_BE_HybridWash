namespace HybridWash.Services.DTOs.Storage;

public sealed record S3FileResult(byte[] Content, string ContentType);
