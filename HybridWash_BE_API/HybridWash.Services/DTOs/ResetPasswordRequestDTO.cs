namespace HybridWash.Services.DTOs
{
    public class ResetPasswordRequestDTO
    {
        public string Email { get; set; } = string.Empty;
        public string OTP { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
