namespace HybridWash.Services.Interfaces
{
    public interface IPlateOcrService
    {
        /// <summary>
        /// Nhận ảnh dưới dạng stream, trả về chuỗi biển số xe đã nhận diện.
        /// Trả null nếu không nhận diện được.
        /// </summary>
        Task<string?> RecognizePlateAsync(Stream imageStream);
    }
}
