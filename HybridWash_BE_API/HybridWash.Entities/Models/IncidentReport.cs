using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HybridWash.Entities.Models;

public partial class IncidentReport
{
    [Key]
    public int ReportId { get; set; }

    public int BookingId { get; set; }

    public int? CustomerId { get; set; }

    public string? ReportedImage1 { get; set; }
    public string? ReportedImage2 { get; set; }
    public string? ReportedImage3 { get; set; }
    public string? ReportedImage4 { get; set; }
    public string? ReportedImage5 { get; set; }

    [Required]
    [MaxLength(1000)]
    public string CustomerNote { get; set; } = null!;

    [MaxLength(20)]
    public string Status { get; set; } = "Pending";

    [MaxLength(1000)]
    public string? ManagerNote { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? ResolvedAt { get; set; }

    [ForeignKey("BookingId")]
    public virtual Booking Booking { get; set; } = null!;

    [ForeignKey("CustomerId")]
    public virtual Customer? Customer { get; set; }
}
