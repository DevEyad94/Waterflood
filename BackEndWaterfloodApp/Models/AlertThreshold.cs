using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using BackEndWaterFloodApp.Models;

namespace BackEndWaterFloodApp.Domain.Entities;

public class AlertThreshold : RefModel
{
    [Key]
    public int Id { get; set; } = 1;

    [Column(TypeName = "decimal(5,2)")]
    public decimal MaxWaterCutPercent { get; set; } = 80m;

    [Column(TypeName = "decimal(12,2)")]
    public decimal MinOilProductionRate { get; set; } = 500m;

    [Column(TypeName = "decimal(12,2)")]
    public decimal MinInjectionRate { get; set; } = 1000m;

    [Column(TypeName = "decimal(10,2)")]
    public decimal MaxInjectionPressure { get; set; } = 2500m;

    [Column(TypeName = "decimal(5,2)")]
    public decimal ProductionDeclinePercent { get; set; } = 20m;
}
