using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndWaterFloodApp.Models;

namespace BackEndWaterFloodApp.Domain.Entities;

public class WaterfloodMeasurementHistory : RefModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    public Guid WellRecordId { get; set; }

    [ForeignKey(nameof(WellRecordId))]
    public WaterfloodRecord WellRecord { get; set; } = null!;

    [Required]
    [MaxLength(50)]
    public string WellName { get; set; } = string.Empty;

    [Required]
    [MaxLength(4)]
    public string WellTypeCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal? InjectionRate { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? OilProductionRate { get; set; }

    [Column(TypeName = "decimal(12,2)")]
    public decimal? WaterProductionRate { get; set; }

    [Column(TypeName = "decimal(5,2)")]
    public decimal? WaterCut { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal? InjectionPressure { get; set; }

    [Required]
    [MaxLength(4)]
    public string WellStatusCode { get; set; } = string.Empty;

    public DateTime MeasurementDate { get; set; }
}
