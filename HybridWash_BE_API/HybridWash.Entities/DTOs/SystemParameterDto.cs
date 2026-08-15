namespace HybridWash.Entities.DTOs;

public class SystemParameterDto
{
    public decimal BikeDepositAmount { get; set; }
    public decimal CarDepositPercentage { get; set; }
    public string ContactPhone { get; set; } = null!;
    public int CancellationRefundDays { get; set; }
}

public class SystemParameterUpdateDto
{
    public decimal BikeDepositAmount { get; set; }
    public decimal CarDepositPercentage { get; set; }
    public string ContactPhone { get; set; } = null!;
    public int CancellationRefundDays { get; set; }
}
