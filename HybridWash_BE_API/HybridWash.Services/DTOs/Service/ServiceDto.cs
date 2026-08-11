namespace HybridWash.Services.DTOs.Service
{
    public class ServiceDto
    {
        public int ServiceId {get; set;}
        public string ServiceName {get; set;} = null!;
        public string? Description {get; set;}
        public decimal Price {get; set;}
        public bool? IsActive {get; set;}
    }
}