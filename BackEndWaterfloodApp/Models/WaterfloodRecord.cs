using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndWaterFloodApp.Models;
using ZSK.Services.ReferenceData.Entities;

namespace BackEndWaterFloodApp.Domain.Entities;

public class WaterfloodRecord : RefModel
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string WellName { get; set; } = string.Empty;

    [Required]
    [MaxLength(4)]
    public string WellTypeCode { get; set; } = string.Empty;

    [ForeignKey(nameof(WellTypeCode))]
    public ZskRefWellType WellType { get; set; } = null!;

    [Required]
    [MaxLength(100)]
    public string FieldName { get; set; } = string.Empty;

    [Column(TypeName = "decimal(9,6)")]
    public decimal Latitude { get; set; }

    [Column(TypeName = "decimal(9,6)")]
    public decimal Longitude { get; set; }

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

    [ForeignKey(nameof(WellStatusCode))]
    public ZskRefWellStatus WellStatus { get; set; } = null!;

    public DateTime MeasurementDate { get; set; }
}
