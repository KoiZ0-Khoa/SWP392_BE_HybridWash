namespace HybridWash.Services.DTOs
{
    public class VehicleResponseDTO
    {
        public int VehicleId { get; set; }
        public string LicensePlate { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
    }
}
