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

        // Regex cho biển số xe Việt Nam
        // Ví dụ: 59A-123.45, 51G-888.88, 29A1-234.56, 92C1-56789
        private static readonly Regex VietPlateRegex = new(
            @"\d{2}[A-Z]\d?[\s\-\.]*\d{3,5}[\s\-\.]*\d{2}",
            RegexOptions.Compiled | RegexOptions.IgnoreCase);

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
                return null;
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

        /// <summary>
        /// Tìm chuỗi biển số xe Việt Nam trong đoạn text OCR trả về.
        /// Chuẩn hóa bằng cách loại bỏ dấu chấm, gạch, khoảng trắng thừa.
        /// </summary>
        private static string? ExtractPlate(string ocrText)
        {
            // Thay newline bằng space để match dễ hơn
            var normalized = ocrText.Replace("\n", " ").Replace("\r", " ");

            var match = VietPlateRegex.Match(normalized);
            if (!match.Success)
                return null;

            // Chuẩn hóa: bỏ dấu chấm, khoảng trắng, chỉ giữ gạch ngang
            var plate = match.Value
                .Replace(".", "")
                .Replace(" ", "")
                .ToUpper();

            // Đảm bảo có đúng 1 dấu gạch ngang ở vị trí đúng
            // Ví dụ: 59A12345 → 59A-12345
            if (!plate.Contains('-'))
            {
                // Tìm vị trí chuyển từ chữ sang số (sau phần prefix)
                var insertPos = 0;
                for (int i = 2; i < plate.Length; i++)
                {
                    if (char.IsLetter(plate[i - 1]) && char.IsDigit(plate[i]) && i > 2)
                    {
                        insertPos = i;
                        break;
                    }
                    // Handle case: 59A1-xxxxx (chữ + 1 số rồi mới tới phần chính)
                    if (i >= 3 && char.IsDigit(plate[i]) && char.IsDigit(plate[i + 1 < plate.Length ? i + 1 : i]))
                    {
                        insertPos = i;
                        break;
                    }
                }
                if (insertPos > 0)
                    plate = plate.Insert(insertPos, "-");
            }

            return plate;
        }
    }
}
