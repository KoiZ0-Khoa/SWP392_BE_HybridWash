namespace HybridWash.Services.DTOs
{
    public class AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
    }
}
