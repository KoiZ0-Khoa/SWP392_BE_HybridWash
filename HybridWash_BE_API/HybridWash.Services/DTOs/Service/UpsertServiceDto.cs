using System.ComponentModel.DataAnnotations;

namespace HybridWash.Services.DTOs.Service;

public class UpsertServiceDto
{
    [Required, MaxLength(100)]
    public string ServiceName { get; set; } = null!;

    [MaxLength(500)]
    public string? Description { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal Price { get; set; }

    public bool IsActive { get; set; } = true;
}
