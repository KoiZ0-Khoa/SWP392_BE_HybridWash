namespace HybridWash.Entities.Models;

public class SystemParameter
{
    public int Id { get; set; }
    public decimal BikeDepositAmount { get; set; }
    public decimal CarDepositPercentage { get; set; }
    public string ContactPhone { get; set; } = null!;
    public int CancellationRefundDays { get; set; }
}
