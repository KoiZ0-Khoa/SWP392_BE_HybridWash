using HybridWash.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HybridWash.Services.Implementations
{
    public class OcrSpacePlateOcrService : IPlateOcrService
    {
        private readonly HttpClient _httpClient;
        private readonly string _apiKey;
        private readonly ILogger<OcrSpacePlateOcrService> _logger;

        public OcrSpacePlateOcrService(
            HttpClient httpClient,
            IConfiguration configuration,
            ILogger<OcrSpacePlateOcrService> logger)
        {
            _httpClient = httpClient;
            _apiKey = configuration["OcrSpace:ApiKey"]
                ?? throw new InvalidOperationException("OcrSpace:ApiKey is not configured in appsettings.json");
            _logger = logger;
        }

        public async Task<string?> RecognizePlateAsync(Stream imageStream)
        {
            try
            {
                // OCR.Space API URL
                var url = "https://api.ocr.space/parse/image";

                // Ensure stream is at the beginning
                if (imageStream.CanSeek)
                {
                    imageStream.Position = 0;
                }

                // Prepare the multipart form data request
                using var content = new MultipartFormDataContent();
                content.Add(new StringContent(_apiKey), "apikey");
                content.Add(new StringContent("eng"), "language");
                content.Add(new StringContent("2"), "OCREngine"); // Engine 2 is better for numbers/alphanumeric

                var streamContent = new StreamContent(imageStream);
                content.Add(streamContent, "file", "plate.jpg");

                // Send request
                var response = await _httpClient.PostAsync(url, content);
                var json = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning("OCR.Space API returned {StatusCode}: {Body}", response.StatusCode, json);
                    throw new Exception($"OCR.Space API Error: {response.StatusCode} - {json}");
                }

                // Parse response
                var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                // Check for OCR.Space application-level errors
                if (root.TryGetProperty("IsErroredOnProcessing", out var isErrored) && isErrored.GetBoolean())
                {
                    var errorMsg = root.TryGetProperty("ErrorMessage", out var msg) ? msg.ToString() : "Unknown Error";
                    _logger.LogWarning("OCR.Space processing error: {ErrorMessage}", errorMsg);
                    throw new Exception($"OCR.Space Error: {errorMsg}");
                }

                if (!root.TryGetProperty("ParsedResults", out var parsedResults) || parsedResults.GetArrayLength() == 0)
                {
                    _logger.LogInformation("No text detected in the image");
                    return null;
                }

                var firstResult = parsedResults[0];
                var fullText = firstResult.TryGetProperty("ParsedText", out var pt) ? pt.GetString() ?? "" : "";
                
                _logger.LogInformation("OCR.Space detected text: {Text}", fullText);

                var plate = ExtractPlate(fullText);
                return plate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to call OCR.Space API");
                throw;
            }
        }

        private static string? ExtractPlate(string ocrText)
        {
            if (string.IsNullOrWhiteSpace(ocrText))
                return null;

            // Xóa tất cả các ký tự không phải chữ cái và số
            var cleaned = Regex.Replace(ocrText, @"[^a-zA-Z0-9]", "").ToUpper();

            if (cleaned.Length < 4)
                return null; // Chuỗi quá ngắn không thể là biển số

            char[] arr = cleaned.ToCharArray();

            // Ký tự 0 và 1 (Mã tỉnh): LUÔN LÀ SỐ
            for (int i = 0; i < 2; i++)
            {
                if (arr[i] == 'O' || arr[i] == 'D' || arr[i] == 'Q') arr[i] = '0';
                else if (arr[i] == 'I' || arr[i] == 'L') arr[i] = '1';
                else if (arr[i] == 'Z') arr[i] = '2';
                else if (arr[i] == 'B') arr[i] = '8';
                else if (arr[i] == 'S') arr[i] = '5';
                else if (arr[i] == 'G') arr[i] = '6';
                else if (arr[i] == 'A') arr[i] = '4';
            }

            // Ký tự thứ 3 (Index 2) (Mã series): LUÔN LÀ CHỮ
            if (arr[2] == '0') arr[2] = 'D'; // 0 rất hay nhầm với D
            else if (arr[2] == '8') arr[2] = 'B';
            else if (arr[2] == '5') arr[2] = 'S';
            else if (arr[2] == '2') arr[2] = 'Z';
            else if (arr[2] == '1') arr[2] = 'I';
            else if (arr[2] == '4') arr[2] = 'A';
            else if (arr[2] == '6') arr[2] = 'G';

            // Từ ký tự thứ 5 (Index 4) trở đi: LUÔN LÀ SỐ (Phần đuôi của biển xe)
            // (Không đụng vào index 3 vì biển 50cc có thể là chữ như 55AA)
            for (int i = 4; i < arr.Length; i++)
            {
                if (arr[i] == 'O' || arr[i] == 'D' || arr[i] == 'Q') arr[i] = '0';
                else if (arr[i] == 'I' || arr[i] == 'L') arr[i] = '1';
                else if (arr[i] == 'Z') arr[i] = '2';
                else if (arr[i] == 'B') arr[i] = '8';
                else if (arr[i] == 'S') arr[i] = '5';
                else if (arr[i] == 'G') arr[i] = '6';
            }

            return new string(arr);
        }
    }
}
