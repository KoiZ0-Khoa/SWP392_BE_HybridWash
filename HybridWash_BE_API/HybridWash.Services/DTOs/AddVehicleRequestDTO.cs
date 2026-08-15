using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs
{
    public class AddVehicleRequestDTO
    {
        [Required(ErrorMessage = "Biển số xe không được để trống.")]
        [StringLength(20, ErrorMessage = "Biển số xe không được vượt quá 20 ký tự.")]
        public string LicensePlate { get; set; } = null!;

        [Required(ErrorMessage = "Loại xe không được để trống.")]
        public string VehicleType { get; set; } = null!; // "Car" hoặc "Bike"
    }
}
