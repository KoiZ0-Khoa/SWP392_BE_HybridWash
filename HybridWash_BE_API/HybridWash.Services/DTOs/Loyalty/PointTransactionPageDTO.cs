namespace HybridWash.Services.DTOs.Loyalty;

public class PointTransactionPageDTO
{
    public IReadOnlyList<PointTransactionDTO> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
