namespace HybridWash.Services.DTOs
{
    public class IssueReceiptRequestDTO
    {
        public int BookingId { get; set; }
        public bool IsCustomerLeaving { get; set; }
        public string? CustomerSignature { get; set; }
    }
}
