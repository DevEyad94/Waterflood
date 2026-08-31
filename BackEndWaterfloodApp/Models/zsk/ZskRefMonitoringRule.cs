using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZSK.Services.ReferenceData.Entities;

[Table("ZSK_Ref_MonitoringRules")]
public class ZskRefMonitoringRule
{
    [Key]
    [MaxLength(50)]
    public string RuleCode { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [MaxLength(10)]
    public string TargetWellType { get; set; } = string.Empty;

    [Column(TypeName = "decimal(12,2)")]
    public decimal DefaultThresholdValue { get; set; }

    [Required]
    [MaxLength(20)]
    public string Severity { get; set; } = string.Empty;
}
