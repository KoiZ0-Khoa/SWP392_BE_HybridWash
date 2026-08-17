namespace HybridWash.Services.DTOs.Loyalty;

public class PointExpiryResultDTO
{
    public int ProcessedCustomers { get; set; }
    public int ProcessedEarnTransactions { get; set; }
    public int ExpiredPoints { get; set; }
}
