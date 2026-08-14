namespace HybridWash.Services.Interfaces
{
    public interface IPlateOcrService
    {
        Task<string?> RecognizePlateAsync(Stream imageStream);
    }
}
