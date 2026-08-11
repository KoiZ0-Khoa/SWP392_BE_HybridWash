using System.ComponentModel;

namespace HybridWash.Services.DTOs
{
    public class LoginRequestDTO
    {
        public string PhoneNumber { get; set; } = null!;
        [DefaultValue("Password@123")]
        public string Password { get; set; } = null!;
    }
}
