namespace HybridWash_BE_API.DTOs
{
    public class LoginRequestDTO
    {
        public string PhoneNumber { get; set; } = null!;
        public string Password { get; set; } = null!;
        public bool IsStaff { get; set; } = false;
    }
}
