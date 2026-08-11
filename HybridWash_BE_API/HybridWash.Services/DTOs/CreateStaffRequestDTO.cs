using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs
{
    public class CreateStaffRequestDTO
    {
        [Required(ErrorMessage = "Họ tên không được để trống.")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Số điện thoại không được để trống.")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Số điện thoại phải bao gồm chính xác 10 chữ số.")]
        public string PhoneNumber { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống.")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*\d)(?=.*[^a-zA-Z\d]).+$", ErrorMessage = "Mật khẩu bắt buộc phải có ít nhất 1 ký tự đặc biệt, 1 chữ in hoa và 1 chữ số.")]
        [System.ComponentModel.DefaultValue("Password@123")]
        public string Password { get; set; } = null!;
        
        [System.ComponentModel.DefaultValue("Staff")]
        public string Role { get; set; } = "Staff";
    }
}
