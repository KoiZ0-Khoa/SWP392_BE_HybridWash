namespace HybridWash.Services.DTOs
{
    public class RegisterRequestDTO
    {
        public string FullName { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
    }
}
