using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HybridWash.Services.Implementations
{
    public class GoogleVisionPlateOcrService : IPlateOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<GoogleVisionPlateOcrService> _logger;

        public GoogleVisionPlateOcrService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<GoogleVisionPlateOcrService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GoogleVision:ApiKey"]
                ?? throw new InvalidOperationException(
                    "GoogleVision:ApiKey is not configured in appsettings.json");
            _logger = logger;
        }

        public async Task<string?> RecognizePlateAsync(Stream imageStream)
        {
            // 1. Đọc ảnh thành base64
            using var memoryStream = new MemoryStream();
            await imageStream.CopyToAsync(memoryStream);
            var base64Image = Convert.ToBase64String(memoryStream.ToArray());

            // 2. Gọi Google Cloud Vision API
            var requestBody = new
            {
                requests = new[]
                {
                    new
                    {
                        image = new { content = base64Image },
                        features = new[]
                        {
                            new { type = "TEXT_DETECTION", maxResults = 10 }
                        }
                    }
                }
            };

            var url = $"https://vision.googleapis.com/v1/images:annotate?key={_apiKey}";

            HttpResponseMessage response;
            try
            {
                response = await _httpClient.PostAsJsonAsync(url, requestBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call Google Vision API");
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Google Vision API returned {StatusCode}: {Body}",
                    response.StatusCode, errorBody);
                throw new Exception($"Google Vision API Error: {response.StatusCode} - {errorBody}");
            }

            // 3. Parse response
            var json = await response.Content.ReadAsStringAsync();
            var doc = JsonDocument.Parse(json);

            var responses = doc.RootElement.GetProperty("responses");
            if (responses.GetArrayLength() == 0)
                return null;

            var firstResponse = responses[0];
            if (!firstResponse.TryGetProperty("textAnnotations", out var annotations)
                || annotations.GetArrayLength() == 0)
            {
                _logger.LogInformation("No text detected in the image");
                return null;
            }

            // textAnnotations[0].description chứa toàn bộ text phát hiện được
            var fullText = annotations[0].GetProperty("description").GetString() ?? "";
            _logger.LogInformation("OCR detected text: {Text}", fullText);

            // 4. Trích xuất biển số từ text bằng Regex
            var plate = ExtractPlate(fullText);
            if (plate != null)
            {
                _logger.LogInformation("Extracted plate: {Plate}", plate);
            }
            else
            {
                _logger.LogInformation("Could not extract plate pattern from OCR text");
            }

            return plate;
        }


        private static string? ExtractPlate(string ocrText)
        {
            if (string.IsNullOrWhiteSpace(ocrText))
                return null;

            // Xóa tất cả các ký tự không phải chữ cái và số (bao gồm cả dấu chấm, gạch, khoảng trắng, ký tự lạ OCR đọc nhầm)
            var cleaned = Regex.Replace(ocrText, @"[^a-zA-Z0-9]", "").ToUpper();

            if (cleaned.Length < 4)
                return null; // Chuỗi quá ngắn không thể là biển số

            return cleaned;
        }
    }
}
